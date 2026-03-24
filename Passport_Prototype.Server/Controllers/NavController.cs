using Microsoft.AspNetCore.Mvc;
using OnlineRegistration.Server.Models;
using SeniorCitizen.Server.Services;
using System.Security.Claims;

namespace OnlineRegistration.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NavController : ControllerBase
    {
        [HttpGet("menu")]
        public IActionResult GetUserMenu()
        {
            // 1. Get the RoleID from the User's Token (1-6 based on your image)
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(roleClaim, out int userRole))
            {
                return Unauthorized(new { message = "Invalid or missing role." });
            }

            // 2. Fetch the full list and filter based on the RoleID
            var filteredMenu = MenuProvider.GetFullMenu()
                .Where(m => m.AllowedRoles.Contains(userRole))
                .Select(m => new {
                    m.Title,
                    m.Url,
                    m.Icon,
                    // Also filter children if you have sub-menus
                    Children = m.Children.Where(c => c.AllowedRoles.Contains(userRole)).ToList()
                })
                .ToList();

            return Ok(filteredMenu);
        }
    }
}
