
using System.Windows.Input;
using MauiMail.Models;
using MauiMail.Services;

namespace MauiMail.ViewModels
{
    public class ComposeViewModel
    {
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public ICommand SendCommand { get; }
        public ICommand AttachCommand { get; }

        private readonly EmailService _emailService;

        public ComposeViewModel()
        {
            _emailService = new EmailService();
            SendCommand = new Command(async () => await SendEmailAsync());
            AttachCommand = new Command(() => { /* Ajouter pièce jointe */ });
        }

        private async Task SendEmailAsync()
        {
            var message = new MauiMail.Models.EmailMessage
            {
                Recipient = this.Recipient,
                Subject = this.Subject,
                Body = this.Body
            };
            await _emailService.SendEmailAsync(message);
        }
    }
}
