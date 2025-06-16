
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

        public IList<Attachment> Attachments { get; } = new List<Attachment>();

        private readonly EmailService _emailService;

        public ComposeViewModel()
        {
            _emailService = new EmailService();
            SendCommand = new Command(async () => await SendEmailAsync());
            AttachCommand = new Command(async () => await PickAttachmentAsync());
        }

        private async Task SendEmailAsync()
        {
            var message = new MauiMail.Models.EmailMessage
            {
                Recipient = this.Recipient,
                Subject = this.Subject,
                Body = this.Body,
                Attachments = this.Attachments.ToList()
            };
            await _emailService.SendEmailAsync(message);
        }

        private async Task PickAttachmentAsync()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    Attachments.Add(new Attachment
                    {
                        FileName = result.FileName,
                        Data = ms.ToArray()
                    });
                }
            }
            catch
            {
                // Ignore errors for now
            }
        }


    }
}
