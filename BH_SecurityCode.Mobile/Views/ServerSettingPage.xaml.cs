using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class ServerSettingPage : ContentPage
    {
        public ServerSettingPage(ServerSettingViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
