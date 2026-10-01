using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class CodeTablePage : ContentPage
    {
        private readonly CodeTableViewModel _vm;

        public CodeTablePage(CodeTableViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        /// <summary>화면을 벗어나면 열어둔 코드를 다시 가린다. (어깨너머 노출 방지)</summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.HideAllCommand.Execute(null);
        }

        private async void OnBackTapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
