using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class SearchPage : ContentPage
    {
        private readonly SearchViewModel _vm;

        public SearchPage(SearchViewModel vm)
        {
            InitializeComponent();
            BindingContext = _vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LoadCommand.Execute(null);
        }
    }
}
