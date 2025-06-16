
using MauiMail.Models;

namespace MauiMail.Services
{
    public class EmailService
    {
        public Task<List<MauiMail.Models.EmailMessage>> GetInboxMessagesAsync()
        {
            return Task.FromResult(new List<MauiMail.Models.EmailMessage>());
        }

        public Task SendEmailAsync(MauiMail.Models.EmailMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
