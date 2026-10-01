using BH_SecurityCode.Mobile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>
    /// 목록·검색 화면의 카드 한 장. 종류가 달라도 같은 템플릿으로 그린다.
    /// 대표 값은 기본으로 가려두고 눈 아이콘으로만 연다.
    /// </summary>
    public sealed partial class VaultRowViewModel : ObservableObject
    {
        private readonly Action<string> _toast;

        public VaultRowViewModel(VaultItem item, bool masked, Action<string> toast)
        {
            Item = item;
            Meta = item.Meta;
            _isRevealed = masked == false;
            _toast = toast;
        }

        public VaultItem Item { get; }
        public CategoryMeta Meta { get; }

        public string Title => Item.Title;
        public string Subtitle => Item.Subtitle;
        public string OwnerName => Item.OwnerName;
        public string ValueLabel => Meta.ValueLabel;
        public bool HasValue => string.IsNullOrWhiteSpace(Item.PrimaryValue) == false;
        public bool HasNote => string.IsNullOrWhiteSpace(Item.Note) == false;
        public string Note => Item.Note;
        public string Flag => Item.Flag;
        public bool HasFlag => Flag.Length > 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayValue))]
        private bool _isRevealed;

        public string DisplayValue =>
            IsRevealed ? Item.PrimaryValue : Mask.Tail(Item.PrimaryValue);

        [RelayCommand]
        private void ToggleReveal() => IsRevealed = !IsRevealed;

        [RelayCommand]
        private async Task CopyValueAsync()
        {
            if (HasValue == false)
                return;

            await Clipboard.SetTextAsync(Item.PrimaryValue);
            _toast($"{ValueLabel} 복사");
        }
    }
}
