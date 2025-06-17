using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MailKit;
using MimeKit;
using MauiMail.Models;
using System.Diagnostics;

namespace MauiMail.Services
{
    public class EmailService
    {

        private const string ImapHostKey = "MailImapHost";
        private const string ImapPortKey = "MailImapPort";
        private const string SmtpHostKey = "MailSmtpHost";
        private const string SmtpPortKey = "MailSmtpPort";
        private const string UsernameKey = "MailUsername";
        private const string PasswordKey = "MailPassword";


        private readonly string _imapHost;
        private readonly int _imapPort;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _username;
        private readonly string _password;

        public EmailService()
        {
            /*
            Preferences.Set(ImapHostKey, "imap.gmail.com");
            Preferences.Set(ImapPortKey, 993);
            Preferences.Set(SmtpHostKey, "smtp.gmail.com");
            Preferences.Set(SmtpPortKey, 587);
            //Preferences.Set(UsernameKey, "xxxxxx@gmail.com");
            //gmail : mot de passe créé pour l'appli (https://myaccount.google.com/apppasswords)) 
            //Preferences.Set(PasswordKey, "lemotdepasse");
            

            _imapHost = Preferences.Get(ImapHostKey, "imap.gmail.com");
            _imapPort = Preferences.Get(ImapPortKey, 993);
            _smtpHost = Preferences.Get(SmtpHostKey, "smtp.example.com");
            _smtpPort = Preferences.Get(SmtpPortKey, 587);
            _username = Preferences.Get(UsernameKey, string.Empty);
            _password = Preferences.Get(PasswordKey, string.Empty);
            */

            /*
            Preferences.Set(ImapHostKey, "imap.free.fr");
            Preferences.Set(ImapPortKey, 993);
            Preferences.Set(SmtpHostKey, "smtp.free.fr");
            Preferences.Set(SmtpPortKey, 587);
            Preferences.Set(UsernameKey, "xxxxxx@free.fr");
            Preferences.Set(PasswordKey, "lemotdepasse");
            */


            _imapHost = Preferences.Get(ImapHostKey, "imap.free.fr");
            _imapPort = Preferences.Get(ImapPortKey, 993);
            _smtpHost = Preferences.Get(SmtpHostKey, "smtp.free.fr");
            _smtpPort = Preferences.Get(SmtpPortKey, 587);
            _username = Preferences.Get(UsernameKey, string.Empty);
            _password = Preferences.Get(PasswordKey, string.Empty);



        }


        public async Task<List<MauiMail.Models.EmailMessage>> GetInboxMessagesAsync()
        {
            var messages = new List<MauiMail.Models.EmailMessage>();
            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true; //todo temporaire
            await client.ConnectAsync(_imapHost, _imapPort, SecureSocketOptions.SslOnConnect);
            if (!string.IsNullOrEmpty(_username))
                await client.AuthenticateAsync(_username, _password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            for (int i = 5; i < 10; i++) //todo  i < inbox.Count
            {
                Debug.WriteLine(i);
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
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;//todo temporaire
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