
namespace MauiMail.Models
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Body { get; set; }
        public DateTime Date { get; set; }
        public List<Attachment> Attachments { get; set; } = new();
    }

    public class Attachment
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }
}
