using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class VaultDetailPage : ContentPage
    {
        private readonly VaultDetailViewModel _vm;

        public VaultDetailPage(VaultDetailViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        /// <summary>수정 화면에서 돌아왔을 때 바뀐 내용을 다시 읽는다.</summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LoadCommand.Execute(null);
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
