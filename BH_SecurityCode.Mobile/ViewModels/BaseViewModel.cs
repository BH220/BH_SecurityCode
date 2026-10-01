using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = "";

        /// <summary>화면 하단에 잠깐 뜨는 알림. 복사 완료 같은 짧은 피드백에 쓴다.</summary>
        [ObservableProperty]
        private string _toastText = "";

        [ObservableProperty]
        private bool _isToastVisible;

        private CancellationTokenSource? _toastCts;

        protected async void ShowToast(string text)
        {
            ToastText = text;
            IsToastVisible = true;

            _toastCts?.Cancel();
            _toastCts = new CancellationTokenSource();
            var token = _toastCts.Token;
            try
            {
                await Task.Delay(1600, token);
                IsToastVisible = false;
            }
            catch (TaskCanceledException)
            {
                // 다음 토스트가 이어서 떴다.
            }
        }

        /// <summary>값을 클립보드에 넣고 토스트를 띄운다.</summary>
        [RelayCommand]
        protected async Task CopyAsync(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            await Clipboard.SetTextAsync(value);
            ShowToast("복사했습니다");
        }
    }
}
