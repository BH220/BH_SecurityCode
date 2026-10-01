using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _vm;

        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.AppearingCommand.Execute(null);
        }

        /// <summary>안드로이드 뒤로 가기로 로그인 화면을 빠져나가지 못하게 막는다.</summary>
        protected override bool OnBackButtonPressed() => true;
    }
}
