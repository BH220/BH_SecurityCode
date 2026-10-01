using System.Collections.ObjectModel;
using BH_SecurityCode.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.ViewModels.BankCode
{
    /// <summary>보안카드 코드 한 개 (순번 + 코드값). API 의 codes 항목 {"순번":"코드"} 에 대응한다.</summary>
    public class BankCodeCodeItem
    {
        public int No { get; set; }
        public string Code { get; set; } = "";
    }

    /// <summary>보안카드 코드 한 칸 (번호 + 코드값) - 격자 셀 표시용</summary>
    public partial class BankCodeCellViewModel : ObservableObject
    {
        private readonly BankCodeCodeItem _item;

        public int No => _item.No;
        public int MaxLength { get; }
        public bool IsReadOnly { get; }

        public string Code
        {
            get => _item.Code;
            set
            {
                string newValue = (value ?? "").Replace(" ", "");
                if (SetProperty(_item.Code, newValue, _item, (m, v) => m.Code = v))
                    OnPropertyChanged(nameof(Display));
            }
        }

        /// <summary>표시용 코드. 4자리는 "12 34" 형태</summary>
        public string Display
        {
            get
            {
                string code = _item.Code;
                if (string.IsNullOrEmpty(code)) return "";
                return code.Length == 4 ? $"{code[..2]} {code[2..]}" : code;
            }
        }

        public BankCodeCellViewModel(BankCodeCodeItem item, int maxLength, bool isReadOnly)
        {
            _item = item;
            MaxLength = maxLength;
            IsReadOnly = isReadOnly;
        }
    }

    public class BankCodeRowViewModel
    {
        public ObservableCollection<BankCodeCellViewModel> Cells { get; } = new();
    }

    /// <summary>
    /// 보안카드 코드 격자 (기존 ctlBankCodeGrid). 최대 40개 코드를 5열로 배열한다.
    /// 세로: 열 방향으로 번호 증가 / 가로: 행 방향으로 번호 증가
    /// </summary>
    public partial class BankCodeGridViewModel : ObservableObject
    {
        public const int TotalCodeCount = 40;

        [ObservableProperty] private ViewTypes _viewType = ViewTypes.세로;
        [ObservableProperty] private int _maxCodeCount = 35;
        [ObservableProperty] private int _maxCodeLength = 4;
        [ObservableProperty] private bool _isReadOnly;

        /// <summary>1~40 번 코드. 화면에는 MaxCodeCount 개만 보인다.</summary>
        public List<BankCodeCodeItem> Codes { get; private set; } = CreateEmptyCodes();
        public ObservableCollection<BankCodeRowViewModel> Rows { get; } = new();

        public BankCodeGridViewModel()
        {
            Rebuild();
        }

        partial void OnViewTypeChanged(ViewTypes value) => Rebuild();
        partial void OnMaxCodeCountChanged(int value) => Rebuild();
        partial void OnMaxCodeLengthChanged(int value) => Rebuild();
        partial void OnIsReadOnlyChanged(bool value) => Rebuild();

        /// <summary>API 의 codes (순번 → 코드) 를 1~40 번에 채운다.</summary>
        public void LoadCodes(IReadOnlyDictionary<int, string> codes)
        {
            var list = CreateEmptyCodes();
            foreach (var pair in codes)
            {
                var target = list.FirstOrDefault(x => x.No == pair.Key);
                if (target != null)
                    target.Code = pair.Value ?? "";
            }
            Codes = list;
            Rebuild();
        }

        public void Reset()
        {
            Codes = CreateEmptyCodes();
            Rebuild();
        }

        /// <summary>저장용. 표시 범위(1~MaxCodeCount) 안의 입력된 코드만 순번 → 코드로 돌려준다.</summary>
        public Dictionary<int, string> GetCodeMap()
        {
            return Codes
                .Where(x => x.No <= MaxCodeCount && string.IsNullOrEmpty(x.Code) == false)
                .ToDictionary(x => x.No, x => x.Code);
        }

        public void Rebuild()
        {
            int maxRow = Math.Clamp(MaxCodeCount / 5, 5, 8);
            Rows.Clear();

            if (ViewType == ViewTypes.세로)
            {
                for (int idx = 1; idx <= maxRow; idx++)
                {
                    var row = new BankCodeRowViewModel();
                    for (int col = 0; col < 5; col++)
                        row.Cells.Add(CreateCell(maxRow * col + idx));
                    Rows.Add(row);
                }
            }
            else
            {
                for (int idx = 0; idx < maxRow; idx++)
                {
                    var row = new BankCodeRowViewModel();
                    for (int col = 1; col <= 5; col++)
                        row.Cells.Add(CreateCell(5 * idx + col));
                    Rows.Add(row);
                }
            }
        }

        private BankCodeCellViewModel CreateCell(int no)
        {
            var item = Codes.First(x => x.No == no);
            return new BankCodeCellViewModel(item, MaxCodeLength, IsReadOnly);
        }

        private static List<BankCodeCodeItem> CreateEmptyCodes()
        {
            var list = new List<BankCodeCodeItem>(TotalCodeCount);
            for (int no = 1; no <= TotalCodeCount; no++)
                list.Add(new BankCodeCodeItem { No = no, Code = "" });
            return list;
        }
    }
}
