using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.ViewModels.BankCode
{
    /// <summary>
    /// 간편보기의 코드 조회 컨트롤 (기존 ctlBankCodeNum).
    /// 코드 번호를 입력하면 해당 코드를 크게 표시하고, 클릭 시 클립보드에 복사한다.
    /// </summary>
    public partial class BankCodeNumViewModel : ObservableObject
    {
        private const string Mask = "**";
        private readonly IClipboardService _clipboard;

        /// <summary>순번 → 코드 (API detail.codes)</summary>
        public IReadOnlyDictionary<int, string> Codes { get; set; } = new Dictionary<int, string>();
        public bool AutoCopy { get; set; } = true;

        /// <summary>코드 번호 2자리 입력 완료 시 발생 (다음 컨트롤로 포커스 이동)</summary>
        public event Action? MoveNextRequested;

        [ObservableProperty] private string _codeNo = "";
        [ObservableProperty] private SimpleViewTypes _viewOption = SimpleViewTypes.모두보기;
        [ObservableProperty] private string _first = Mask;
        [ObservableProperty] private string _second = Mask;
        [ObservableProperty] private string _third = Mask;
        [ObservableProperty] private bool _showPair = true;
        [ObservableProperty] private bool _showSingle;
        [ObservableProperty] private bool _isInputFocused;

        public BankCodeNumViewModel(IClipboardService clipboard)
        {
            _clipboard = clipboard;
        }

        partial void OnCodeNoChanged(string value)
        {
            FindSetNum();
            if (value.Length >= 2)
                MoveNextRequested?.Invoke();
        }

        partial void OnViewOptionChanged(SimpleViewTypes value) => FindSetNum();

        public void Focus()
        {
            IsInputFocused = false;
            IsInputFocused = true;
        }

        [RelayCommand]
        private void Copy(string? text)
        {
            if (AutoCopy == false || string.IsNullOrEmpty(text) || text == Mask)
                return;
            _clipboard.SetText(text.Replace("*", ""));
        }

        private void FindSetNum()
        {
            string? code = null;
            if (int.TryParse(CodeNo, out int no))
                Codes.TryGetValue(no, out code);

            if (string.IsNullOrEmpty(code))
            {
                First = Second = Third = Mask;
                ShowPair = true;
                ShowSingle = false;
                return;
            }

            if (code.Length == 4)
            {
                First = code[..2];
                Second = code[2..];
                ShowPair = true;
                ShowSingle = false;
                if (ViewOption == SimpleViewTypes.앞2자리만보기) Second = Mask;
                else if (ViewOption == SimpleViewTypes.뒤2자리만보기) First = Mask;
            }
            else
            {
                Third = ViewOption switch
                {
                    SimpleViewTypes.앞2자리만보기 => code[..Math.Min(2, code.Length)] + new string('*', Math.Max(0, code.Length - 2)),
                    SimpleViewTypes.뒤2자리만보기 => new string('*', Math.Max(0, code.Length - 2)) + code[Math.Max(0, code.Length - 2)..],
                    _ => code,
                };
                ShowPair = false;
                ShowSingle = true;
            }
        }
    }
}
