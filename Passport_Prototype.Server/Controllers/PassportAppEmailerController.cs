using Microsoft.AspNetCore.Mvc;
using OnlineRegistration.Server.Data;
using OnlineRegistration.Server.Models;
using OnlineRegistration.Server.Services.Interfaces;
using Passport_Prototype.Server.DTOs;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace Passport_Prototype.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassportAppEmailerController : ControllerBase
    {
        private readonly IEmailQueue _emailQueue;
        private readonly AppDbContext _context;

        public PassportAppEmailerController(IEmailQueue emailQueue, AppDbContext context)
        {
            _emailQueue = emailQueue;
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendAppEmail(PassportAppEmailDTO passportAppEmailDTO)
        {
            var random = new Random();

            long min = 1_000_000_000;
            long max = 9_999_999_999;

            long number = (long)(random.NextDouble() * (max - min)) + min;

            var emailBody = $"""
                                <body style="font-family: Arial, sans-serif; background-color: #f4f6f8; margin: 0; padding: 20px;">
                  <table align="center" width="600" cellpadding="0" cellspacing="0" style="background-color: #ffffff; padding: 20px; border: 1px solid #ddd;">

                    <tr>
                      <td style="font-size: 14px; color: #333;">
                        <p>Dear <strong>{passportAppEmailDTO.AccountName}</strong>,</p>

                        <p style="color: #c0392b; font-weight: bold;">
                          THIS IS NOT YOUR BOOKING CONFIRMATION.
                        </p>

                        <p>
                          You are receiving this message in connection with your attempt to book a provisional schedule for passport services at
                          <strong>{passportAppEmailDTO.Site}</strong> on <strong>{passportAppEmailDTO.Schedule}</strong>.
                        </p>

                        <p>
                          To confirm your appointment, please pay the amount of <strong>{passportAppEmailDTO.Amount}</strong> to your selected accredited payment center using the
                          reference number below within 24 hours upon receipt of this email. Failure to do so will automatically cancel the provisional booking to give way to other passport clients.
                        </p>

                        <p style="text-align: center; font-size: 18px; font-weight: bold; color: #2c3e50; margin: 30px 0;">
                          Reference Number: <span style="color: #000;">{number.ToString()}</span>
                        </p>

                        <h4 style="margin-top: 30px;">General Guidelines:</h4>
                        <ul style="padding-left: 20px; line-height: 1.6;">
                          <li>
                            Pay in CASH at Bayad Center Branches and Authorized Partners (Robinsons Malls & Supermarkets, LBC, eBiz, Perahub, USSC, TrueMoney, Villarica, etc.), or through 7-11 and ECPay outlets. You may also pay via Bayad App available in both iOS and Android devices.
                          </li>
                          <li>Confirmation Payments are processed once paid.</li>
                          <li>We will send a confirmation email to you once processed.</li>
                          <li>
                            Pay the exact amount. Any excess payment will be forfeited. Payments less than the total amount indicated will not be processed.
                          </li>
                          <li>
                            Amount is inclusive of convenience fee. If you are paying for multiple reference numbers, pay separately for each reference number. One transaction per reference number.
                          </li>
                          <li>Make sure to get a reference number before paying.</li>
                          <li>
                            A reference number can only be used once. If you made a short payment by mistake, do not try to correct it by making another bills payment with the same reference number. Contact our helpdesk immediately.
                          </li>
                        </ul>

                      </td>
                    </tr>

                  </table>
                </body>
                """;


            var userIdstring = User.FindFirstValue("id");

            if (!int.TryParse(userIdstring, out int userId))
                return BadRequest("Invalid recipient");

            var user = await _context.Users.FindAsync(userId);

            Debug.WriteLine(user.Email);

            _emailQueue.QueueEmail(new EmailMessage
            {
                To = user.Email,
                Subject = "Passport Application Notification",
                Body = emailBody
            });
            return NoContent();
        }
    }
}
