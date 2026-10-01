using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>
    /// 서버 주소 설정. 데스크톱 ServerSettingWindow 와 같은 역할이고 검증 규칙도 동일하다.
    /// </summary>
    public sealed partial class ServerSettingViewModel : BaseViewModel
    {
        private readonly IAppSettings _settings;

        public ServerSettingViewModel(IAppSettings settings)
        {
            _settings = settings;
            _address = settings.ServerAddress;
            Title = "서버 주소";
        }

        [ObservableProperty]
        private string _address = "";

        [ObservableProperty]
        private string _errorText = "";

        public bool HasError => string.IsNullOrEmpty(ErrorText) == false;

        partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(HasError));

        partial void OnAddressChanged(string value) => ErrorText = "";

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IAppSettings.IsValidAddress(Address, out string error) == false)
            {
                ErrorText = error;
                return;
            }

            _settings.ServerAddress = Address;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void UseSample(string? sample)
        {
            if (string.IsNullOrEmpty(sample) == false)
                Address = sample;
        }
    }
}
