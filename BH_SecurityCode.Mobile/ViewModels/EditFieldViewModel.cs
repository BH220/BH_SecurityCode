using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    public enum EditKind
    {
        Text,
        Multiline,
        Choice,
        Date,
    }

    /// <summary>선택 목록의 한 항목. 값은 공통코드(CdBank / CdOwner / ...)의 정수값이다.</summary>
    public sealed class ChoiceOption
    {
        public ChoiceOption(int value, string text)
        {
            Value = value;
            Text = text;
        }

        public int Value { get; }
        public string Text { get; }

        public override string ToString() => Text;
    }

    /// <summary>
    /// 편집 화면의 입력 한 칸. 종류(<see cref="EditKind"/>)에 따라 다른 템플릿으로 그린다.
    /// 값 → 모델 매핑은 VaultEditViewModel 이 필드 참조를 직접 들고 처리한다.
    /// </summary>
    public sealed partial class EditFieldViewModel : ObservableObject
    {
        public required string Label { get; init; }
        public required EditKind Kind { get; init; }

        public string Placeholder { get; init; } = "";
        public bool Required { get; init; }
        public Keyboard Keyboard { get; init; } = Keyboard.Default;
        public int MaxLength { get; init; } = 200;

        /// <summary>날짜를 비워둘 수 있는지 (만기일자 · 만료일)</summary>
        public bool DateOptional { get; init; }

        public IReadOnlyList<ChoiceOption> Options { get; init; } = Array.Empty<ChoiceOption>();

        [ObservableProperty]
        private string _text = "";

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        /// <summary>날짜가 지정되었는지. false 면 서버에 빈 값으로 보낸다.</summary>
        [ObservableProperty]
        private bool _hasDate;

        [ObservableProperty]
        private ChoiceOption? _choice;

        [ObservableProperty]
        private string _error = "";

        public bool HasError => Error.Length > 0;

        public string LabelText => Required ? Label + " *" : Label;

        partial void OnErrorChanged(string value) => OnPropertyChanged(nameof(HasError));

        partial void OnTextChanged(string value) => Error = "";

        partial void OnChoiceChanged(ChoiceOption? value) => Error = "";

        /// <summary>선택된 코드값. 선택이 없으면 0.</summary>
        public int ChoiceValue => Choice?.Value ?? 0;

        /// <summary>서버로 보낼 날짜. 지정하지 않았으면 null.</summary>
        public DateTime? DateValue => HasDate ? Date : null;

        public void SetChoice(int value)
        {
            Choice = Options.FirstOrDefault(x => x.Value == value) ?? Options.FirstOrDefault();
        }

        public void SetDate(DateTime? value)
        {
            HasDate = value.HasValue;
            if (value.HasValue)
                Date = value.Value;
        }

        [RelayCommand]
        private void PickDate() => HasDate = true;

        [RelayCommand]
        private void ClearDate() => HasDate = false;
    }
}
