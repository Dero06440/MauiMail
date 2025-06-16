
namespace MauiMail.Views
{
    public partial class InboxPage : ContentPage
    {
        public InboxPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ViewModels.InboxViewModel vm && vm.RefreshCommand.CanExecute(null))
            {
                vm.RefreshCommand.Execute(null);
            }
        }
    }
}
