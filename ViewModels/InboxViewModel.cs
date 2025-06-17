
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using MauiMail.Models;
using MauiMail.Services;

namespace MauiMail.ViewModels
{
    public class InboxViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MauiMail.Models.EmailMessage> Messages { get; set; } = new();
        public ICommand RefreshCommand { get; }
        private MauiMail.Models.EmailMessage? _selectedMessage;
        public MauiMail.Models.EmailMessage? SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                if (_selectedMessage != value)
                {
                    _selectedMessage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedMessage)));
                }
            }
        }

        private readonly EmailService _emailService;

        public event PropertyChangedEventHandler? PropertyChanged;

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
