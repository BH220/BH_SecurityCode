using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _vm;

        public SettingsPage(SettingsViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // SD카드는 꽂았다 뺄 수 있고 캐시 용량도 계속 변한다. 열 때마다 다시 확인한다.
            _vm.AppearingCommand.Execute(null);
        }
    }
}
