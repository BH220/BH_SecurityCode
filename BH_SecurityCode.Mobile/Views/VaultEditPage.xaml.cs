using BH_SecurityCode.Mobile.ViewModels;

namespace BH_SecurityCode.Mobile.Views
{
    public partial class VaultEditPage : ContentPage
    {
        public VaultEditPage(VaultEditViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
