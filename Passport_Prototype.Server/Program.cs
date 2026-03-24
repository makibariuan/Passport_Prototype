using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services;
using OnlineRegistration.Server.Services.Interfaces;
using SixLabors.ImageSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using QuestPDF.Infrastructure;
using System.Reflection;
using SeniorCitizen.Server.Data;

QuestPDF.Settings.License = LicenseType.Community;


var builder = WebApplication.CreateBuilder(args);
// ------------------ Services ------------------

// Users Table
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Personal Data Sheet
builder.Services.AddDbContext<EmployeesDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<PsgcDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen(options =>
{
    // 1. Define the API Info
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Makati Citizen and Employee Onboarding API",
        Version = "v1",
        Description = "API for Citizen and Employee Onboarding"
    });

    // 2. Add Security Definition (Bearer Token)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below.",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // 3. Feed the XML comments to Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Check if file exists to prevent errors during first-time builds
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IEmailQueue, EmailBackgroundQueue>(); // Service shared with controller
builder.Services.AddHostedService<EmailSender>(); // email sender
builder.Services.AddScoped<IFileService, FileService>(); // For file handling
builder.Services.AddScoped<IPgpService, PgpService>(); // For PGP encryption
builder.Services.AddScoped<IPasswordHasher<UsersEmployee>, PasswordHasher<UsersEmployee>>(); // For password hashing


// AFIS queue
builder.Services.AddSingleton<AfisQueueService>(); // Service shared with controller
builder.Services.AddHostedService<AfisQueueWorker>(); // Background worker

// CORS for local dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// ------------------ Auth (JWT + Single Session) ------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };

    // 🔑 Custom validation to enforce single active session
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail("Invalid token");
                return;
            }

            var user = await db.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                context.Fail("User not found");
                return;
            }

            // ✅ Compare against Authorization header (raw string from request)
            var requestToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(user.ActiveToken) || user.ActiveToken != requestToken)
            {
                context.Fail("Session expired");
            }
        },
        OnAuthenticationFailed = context =>
        {
            context.NoResult();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { message = "Session expired" });
        }
    };
});


builder.Services.AddAuthorization(options =>
{
    // Policy 1: Highest level access for creating/modifying users
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireClaim("userrole", "1"));

    // Policy 2: Access for user creation and modification (Super Admin and System User)
    options.AddPolicy("RequireManagementAccess", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "userrole" && (c.Value == "1" || c.Value == "2"))));

    // Policy 3: Access for viewing administrative lists (Super Admin, System User, HR)
    options.AddPolicy("RequireAdministrativeView", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "userrole" && (c.Value == "1" || c.Value == "2" || c.Value == "6"))));

    // Policy 4: Access for employee-level services (Role 1, 2, 4)
    options.AddPolicy("RequireEmployeeAccess", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "userrole" && (c.Value == "1" || c.Value == "2" || c.Value == "4"))));

    // Policy 5: Access for public/citizen-level services (Role 5)
    options.AddPolicy("RequireCitizenAccess", policy =>
        policy.RequireClaim("userrole", "5"));
});


builder.Services.AddAuthorization();

var app = builder.Build();

// ------------------ Middleware Pipeline ------------------

app.UseRouting();
app.UseCors("DevPolicy");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map API controllers
app.MapControllers();

// ------------------ Custom Fallback for SPA ------------------

app.MapFallback(async context =>
{
    var path = context.Request.Path;

    // If it's an API path, return 404 JSON instead of serving index.html
    if (path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 404;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = "API endpoint not found." });
        return;
    }

    // Otherwise, serve the SPA
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
