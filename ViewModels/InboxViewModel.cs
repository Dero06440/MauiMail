
using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiMail.Models;
using MauiMail.Services;

namespace MauiMail.ViewModels
{
    public class InboxViewModel
    {
        public ObservableCollection<MauiMail.Models.EmailMessage> Messages { get; set; } = new();
        public ICommand RefreshCommand { get; }

        private readonly EmailService _emailService;

        public InboxViewModel()
        {
            _emailService = new EmailService();
            RefreshCommand = new Command(async () => await LoadMessagesAsync());
        }

        private async Task LoadMessagesAsync()
        {
            var emails = await _emailService.GetInboxMessagesAsync();
            Messages.Clear();
            foreach (var email in emails)
                Messages.Add(email);
        }
    }
}
