using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace FastBite.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Dev fallback: don't crash registration if no real mail provider is configured.
            _logger.LogInformation("EmailSender stub called. To: {Email}, Subject: {Subject}", email, subject);
            return Task.CompletedTask;
        }
    }
}