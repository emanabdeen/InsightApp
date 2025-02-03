using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using InsightApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;

namespace InsightApp.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static MailAddress fromAddress = new MailAddress("snoekhook@gmail.com", "InsightCVGS-NoReply");
        private SmtpClient smptClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = true,
        };



        public async Task SendEmailAsync(string toAddress, string subjectLine, string messageBody)
        {
            smptClient.Credentials = new NetworkCredential(fromAddress.Address, _configuration["EmailServiceConfig:AppPassword"]);
            MailMessage newEmail = new MailMessage(fromAddress.Address, toAddress, subjectLine, messageBody)
            {
                IsBodyHtml = true
            };
            await smptClient.SendMailAsync(newEmail);
        }
    }
}
