using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MailKit;
using MimeKit;
using MauiMail.Models;

namespace MauiMail.Services
{
    public class EmailService
    {
        private readonly string _imapHost;
        private readonly int _imapPort;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _username;
        private readonly string _password;

        public EmailService()
        {
            // These values can be provided through configuration or environment variables
            _imapHost = Environment.GetEnvironmentVariable("MAIL_IMAP_HOST") ?? "imap.example.com";
            _imapPort = int.TryParse(Environment.GetEnvironmentVariable("MAIL_IMAP_PORT"), out var imap) ? imap : 993;
            _smtpHost = Environment.GetEnvironmentVariable("MAIL_SMTP_HOST") ?? "smtp.example.com";
            _smtpPort = int.TryParse(Environment.GetEnvironmentVariable("MAIL_SMTP_PORT"), out var smtp) ? smtp : 587;
            _username = Environment.GetEnvironmentVariable("MAIL_USERNAME") ?? string.Empty;
            _password = Environment.GetEnvironmentVariable("MAIL_PASSWORD") ?? string.Empty;
        }

        public async Task<List<MauiMail.Models.EmailMessage>> GetInboxMessagesAsync()
        {
            var messages = new List<MauiMail.Models.EmailMessage>();
            using var client = new ImapClient();
            await client.ConnectAsync(_imapHost, _imapPort, SecureSocketOptions.SslOnConnect);
            if (!string.IsNullOrEmpty(_username))
                await client.AuthenticateAsync(_username, _password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            for (int i = 0; i < inbox.Count; i++)
            {
                var mime = await inbox.GetMessageAsync(i);
                var email = new MauiMail.Models.EmailMessage
                {
                    Subject = mime.Subject ?? string.Empty,
                    Sender = mime.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                    Recipient = mime.To.Mailboxes.FirstOrDefault()?.Address ?? string.Empty,
                    Body = mime.TextBody ?? mime.HtmlBody ?? string.Empty,
                    Date = mime.Date.DateTime,
                };

                foreach (var attachment in mime.Attachments.OfType<MimePart>())
                {
                    using var stream = new MemoryStream();
                    await attachment.Content.DecodeToAsync(stream);
                    email.Attachments.Add(new Attachment
                    {
                        FileName = attachment.FileName ?? "attachment",
                        Data = stream.ToArray()
                    });
                }

                messages.Add(email);
            }

            await client.DisconnectAsync(true);
            return messages;
        }

        public async Task SendEmailAsync(MauiMail.Models.EmailMessage message)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            if (!string.IsNullOrEmpty(_username))
                await client.AuthenticateAsync(_username, _password);

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_username));
            mimeMessage.To.Add(MailboxAddress.Parse(message.Recipient));
            mimeMessage.Subject = message.Subject ?? string.Empty;

            var bodyBuilder = new BodyBuilder { TextBody = message.Body ?? string.Empty };
            foreach (var att in message.Attachments)
            {
                bodyBuilder.Attachments.Add(att.FileName, att.Data);
            }
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }

    }
}