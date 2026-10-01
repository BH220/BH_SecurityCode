using System.Collections.ObjectModel;
using BH_SecurityCode.Mobile.Models;
using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>보안카드 코드 한 칸. 눌러서 열고 다시 눌러 닫는다.</summary>
    public sealed partial class CodeCellViewModel : ObservableObject
    {
        private readonly Func<CodeRevealMode> _mode;

        public CodeCellViewModel(int no, string code, Func<CodeRevealMode> mode)
        {
            No = no;
            Code = code ?? "";
            _mode = mode;
        }

        public int No { get; }
        public string Code { get; }
        public string NoText => No.ToString("00");

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Display))]
        private bool _isRevealed;

        public string Display
        {
            get
            {
                if (IsRevealed == false)
                    return new string('•', Math.Max(Code.Length, 4));

                return _mode() switch
                {
                    CodeRevealMode.앞2자리 => Code.Length >= 2 ? Code.Substring(0, 2) + "••" : Code,
                    CodeRevealMode.뒤2자리 => Code.Length >= 2 ? "••" + Code.Substring(Code.Length - 2) : Code,
                    _ => Code,
                };
            }
        }

        public void Refresh() => OnPropertyChanged(nameof(Display));

        [RelayCommand]
        private void Toggle() => IsRevealed = !IsRevealed;
    }

    /// <summary>데스크톱 SimpleViewTypes(간편보기 표시 옵션) 와 같다.</summary>
    public enum CodeRevealMode
    {
        모두보기,
        앞2자리,
        뒤2자리,
    }

    /// <summary>
    /// 보안카드 코드표. 데스크톱의 간편보기(F1) · 상세보기(F2) 두 창을 한 화면으로 합쳤다.
    /// 기본은 전부 가려 두고, 필요한 칸만 눌러서 연다. (어깨너머로 보이는 것을 막기 위함)
    /// </summary>
    public sealed partial class CodeTableViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IVaultDataService _data;

        public CodeTableViewModel(IVaultDataService data)
        {
            _data = data;
        }

        public ObservableCollection<CodeCellViewModel> Cells { get; } = new();

        [ObservableProperty]
        private int _itemKey;

        [ObservableProperty]
        private string _cardName = "";

        [ObservableProperty]
        private string _bankName = "";

        [ObservableProperty]
        private string _alignText = "";

        [ObservableProperty]
        private string _errorText = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SerialDisplay))]
        private bool _isSerialRevealed;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SerialDisplay))]
        private string _serial = "";

        public string SerialDisplay => IsSerialRevealed ? Serial : Mask.Tail(Serial, 2);

        public bool HasError => ErrorText.Length > 0;

        [ObservableProperty]
        private CodeRevealMode _revealMode = CodeRevealMode.모두보기;

        [ObservableProperty]
        private bool _isAllRevealed;

        /// <summary>표시 방식 칩. 데스크톱 SimpleViewTypes 선택과 같다.</summary>
        public ObservableCollection<ChipViewModel> Modes { get; } = new()
        {
            new ChipViewModel("모두 보기", true),
            new ChipViewModel("앞 2자리"),
            new ChipViewModel("뒤 2자리"),
        };

        [RelayCommand]
        private void SelectMode(ChipViewModel? chip)
        {
            if (chip == null)
                return;

            foreach (var mode in Modes)
                mode.IsSelected = ReferenceEquals(mode, chip);

            RevealMode = chip.Text switch
            {
                "앞 2자리" => CodeRevealMode.앞2자리,
                "뒤 2자리" => CodeRevealMode.뒤2자리,
                _ => CodeRevealMode.모두보기,
            };
        }

        /// <summary>Shell 이 넘겨준 codes?key=3 을 받는다.</summary>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("key", out object? value) &&
                int.TryParse(value?.ToString(), out int key))
                ItemKey = key;
        }

        partial void OnItemKeyChanged(int value) => _ = LoadAsync();

        partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(HasError));

        partial void OnRevealModeChanged(CodeRevealMode value)
        {
            foreach (var cell in Cells)
                cell.Refresh();
        }

        partial void OnIsAllRevealedChanged(bool value)
        {
            foreach (var cell in Cells)
                cell.IsRevealed = value;
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy || ItemKey == 0)
                return;

            IsBusy = true;
            ErrorText = "";
            try
            {
                var result = await _data.GetSecurityCodeAsync(ItemKey);
                if (result.Success == false || result.Data == null)
                {
                    ErrorText = result.Message;
                    return;
                }

                var item = result.Data;
                CardName = item.name ?? "";
                BankName = $"{item.bank_type}은행";
                Serial = item.serial ?? "";
                AlignText = $"{item.code_align} 배열 · 코드 {item.code_qty}개";
                Title = CardName;

                Cells.Clear();
                foreach (var pair in item.GetCodeMap().OrderBy(x => x.Key))
                    Cells.Add(new CodeCellViewModel(pair.Key, pair.Value, () => RevealMode));

                if (Cells.Count == 0)
                    ErrorText = "이 보안카드에 등록된 코드가 없습니다.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task RetryAsync() => LoadAsync();

        [RelayCommand]
        private void HideAll()
        {
            IsAllRevealed = false;
            foreach (var cell in Cells)
                cell.IsRevealed = false;

            IsSerialRevealed = false;
        }

        [RelayCommand]
        private void ToggleSerial() => IsSerialRevealed = !IsSerialRevealed;

        [RelayCommand]
        private async Task CopySerialAsync()
        {
            if (string.IsNullOrEmpty(Serial))
                return;

            await Clipboard.SetTextAsync(Serial);
            ShowToast("일련번호 복사");
        }
    }
}
