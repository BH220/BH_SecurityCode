using System.Collections.ObjectModel;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.ViewModels.BankCode
{
    /// <summary>
    /// 보안카드 간편보기 (기존 frmBankCodeViewSimple).
    /// 일련번호는 클릭한 자리만 표시되고, 두 개의 코드 조회 컨트롤로 앞/뒤 2자리를 확인한다.
    /// </summary>
    public partial class BankCodeSimpleViewViewModel : DialogViewModelBase
    {
        private readonly IBankCodeManager _manager;
        private readonly IDialogService _dialog;

        public ObservableCollection<SerialCellViewModel> SerialCells { get; } = new();
        public BankCodeNumViewModel Num1 { get; }
        public BankCodeNumViewModel Num2 { get; }

        [ObservableProperty] private bool _autoCopy = true;
        [ObservableProperty] private string _bankCodeName = "";
        [ObservableProperty] private string _bankName = "";

        public BankCodeSimpleViewViewModel(IBankCodeManager manager, IDialogService dialog, IClipboardService clipboard)
        {
            _manager = manager;
            _dialog = dialog;

            Num1 = new BankCodeNumViewModel(clipboard) { ViewOption = SimpleViewTypes.앞2자리만보기 };
            Num2 = new BankCodeNumViewModel(clipboard) { ViewOption = SimpleViewTypes.뒤2자리만보기 };
            Num1.MoveNextRequested += () => Num2.Focus();
            Num2.MoveNextRequested += () => Num1.Focus();
        }

        partial void OnAutoCopyChanged(bool value)
        {
            Num1.AutoCopy = value;
            Num2.AutoCopy = value;
        }

        public async Task<bool> LoadAsync(int bankCodeNum)
        {
            var info = await _manager.GetDetailAsync(bankCodeNum);
            if (info == null)
            {
                _dialog.ShowError("보안카드정보를 불러오는데 실패 하였습니다.");
                return false;
            }

            BankCodeName = info.name ?? "";
            BankName = info.bank_type.ToString();
            Title = $"{BankCodeName}[{BankName}] 보안카드";

            SerialCells.Clear();
            foreach (var cell in SerialCellViewModel.Build(info.serial, revealed: false))
                SerialCells.Add(cell);

            var codes = info.GetCodeMap();
            Num1.Codes = codes;
            Num2.Codes = codes;
            Num1.Focus();
            return true;
        }
    }
}
