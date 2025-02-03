using Microsoft.AspNetCore.Identity.UI.Services;

namespace InsightApp.Services
{
    public class EmailSender :IEmailSender
    {
        private readonly EmailService _emailService;
        
        public EmailSender(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string toAddress, string subjectLine, string messageBody)
        {
            await _emailService.SendEmailAsync(toAddress, subjectLine, messageBody);
        }
    }
}