using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class VaultListPage : ContentPage
    {
        private readonly VaultListViewModel _vm;

        public VaultListPage(VaultListViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        /// <summary>추가·수정·삭제 후 돌아왔을 때 목록을 다시 채운다. (첫 진입은 쿼리에서 이미 불러온다)</summary>
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
