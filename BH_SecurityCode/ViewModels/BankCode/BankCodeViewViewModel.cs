using System.Collections.ObjectModel;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using BH_SecurityCode.ViewModels.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.ViewModels.BankCode
{
    /// <summary>보안카드 상세보기 (기존 frmBankCodeView) - 일련번호, 전체 코드, 첨부 이미지를 표시한다.</summary>
    public partial class BankCodeViewViewModel : DialogViewModelBase
    {
        private readonly IBankCodeManager _manager;
        private readonly IDialogService _dialog;

        public BankCodeGridViewModel Grid { get; } = new() { IsReadOnly = true };
        public ObservableCollection<SerialCellViewModel> SerialCells { get; } = new();

        /// <summary>첨부 이미지 (읽기 전용: 더블클릭으로 확대 보기만 가능)</summary>
        public ImageGridViewModel<BankCodeImageInfo> Images { get; }

        [ObservableProperty] private string _bankCodeName = "";
        [ObservableProperty] private string _bankName = "";
        [ObservableProperty] private string _ownerName = "";
        [ObservableProperty] private string _formatText = "";

        public BankCodeViewViewModel(IBankCodeManager manager, IDialogService dialog, IImageManager images)
        {
            _manager = manager;
            _dialog = dialog;
            Images = new ImageGridViewModel<BankCodeImageInfo>(dialog, images, CdImageType.보안카드) { IsReadOnly = true };
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
            OwnerName = info.owner_type.ToString();
            Title = $"{BankCodeName}[{BankName}] 보안카드";
            FormatText = $"4자리 {info.code_qty}개 · {info.code_align}";

            SerialCells.Clear();
            foreach (var cell in SerialCellViewModel.Build(info.serial, revealed: true))
                SerialCells.Add(cell);

            Grid.ViewType = info.code_align == CdAlign.가로 ? ViewTypes.가로 : ViewTypes.세로;
            Grid.MaxCodeLength = 4;
            Grid.MaxCodeCount = info.code_qty;
            Grid.LoadCodes(info.GetCodeMap());

            Images.Load(info.images);
            return true;
        }
    }
}
