using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.ViewModels.BankCode
{
    /// <summary>
    /// 일련번호 한 자리. 정방향/역방향 번호와 문자를 갖는다. 10칸 우측 정렬로 표시된다.
    /// </summary>
    public partial class SerialCellViewModel : ObservableObject
    {
        public string Asc { get; }
        public string Desc { get; }
        public string Char { get; }
        public bool HasValue => string.IsNullOrEmpty(Char) == false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Display))]
        private bool _isRevealed;

        public string Display
        {
            get
            {
                if (HasValue == false) return "";
                return IsRevealed ? Char : "-";
            }
        }

        private SerialCellViewModel(string asc, string desc, string ch, bool revealed)
        {
            Asc = asc;
            Desc = desc;
            Char = ch;
            _isRevealed = revealed;
        }

        [RelayCommand]
        private void Toggle()
        {
            if (HasValue)
                IsRevealed = !IsRevealed;
        }

        /// <summary>일련번호를 10칸에 우측 정렬로 배치한다.</summary>
        public static List<SerialCellViewModel> Build(string? serial, bool revealed)
        {
            serial = (serial ?? "").Trim();
            if (serial.Length > 10)
                serial = serial[^10..];

            var cells = new List<SerialCellViewModel>(10);
            int offset = 10 - serial.Length;
            for (int idx = 0; idx < 10; idx++)
            {
                int pos = idx - offset;
                if (pos < 0)
                    cells.Add(new SerialCellViewModel("", "", "", revealed));
                else
                    cells.Add(new SerialCellViewModel((pos + 1).ToString(), (serial.Length - pos).ToString(), serial[pos].ToString(), revealed));
            }
            return cells;
        }
    }
}
