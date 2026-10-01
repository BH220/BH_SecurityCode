using System.Collections.ObjectModel;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using BH_SecurityCode.ViewModels.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.ViewModels.BankBook
{
    /// <summary>통장 추가/수정 (기존 frmBankBookInfo). API 응답 <see cref="BankBookInfo"/> 기준으로 동작한다.</summary>
    public partial class BankBookEditViewModel : EditViewModelBase
    {
        private readonly IBankBookManager _manager;

        /// <summary>수정 대상 bank_book_num. 0 이면 신규</summary>
        private int _bankBookNum;

        public ObservableCollection<CodeInfo> Banks { get; } = new();
        public ImageGridViewModel<BankBookImageInfo> Images { get; }

        [ObservableProperty] private string _name = "";
        [ObservableProperty] private CodeInfo? _selectedBank;
        /// <summary>예금주 (API 필드 owner). 응답에는 내려오지 않으므로 수정 시 빈 값으로 열린다.</summary>
        [ObservableProperty] private string _holderName = "";
        [ObservableProperty] private string _accountNo = "";
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private string _note = "";
        [ObservableProperty] private bool _isNameFocused;

        public BankBookEditViewModel(IBankBookManager manager, ICodeManager codes, IDialogService dialog, IImageManager images)
            : base(dialog, codes)
        {
            _manager = manager;
            Images = new ImageGridViewModel<BankBookImageInfo>(dialog, images, CdImageType.통장);
        }

        /// <summary>bankBookNum 이 null/0 이면 추가, 아니면 상세 API 로 불러와 수정 모드로 연다.</summary>
        public async Task<bool> LoadAsync(int? bankBookNum)
        {
            await LoadOwnersAsync();
            Banks.Clear();
            foreach (var code in await Codes.GetCodesAsync(CodeTypes.Bank))
                Banks.Add(code);

            if (bankBookNum == null || bankBookNum <= 0)
            {
                IsNew = true;
                Title = "통장정보 추가";
                _bankBookNum = 0;
                Images.Clear();
            }
            else
            {
                var info = await _manager.GetAsync(bankBookNum.Value);
                if (info == null)
                {
                    Dialog.ShowError("통장정보를 불러오는데 실패 하였습니다.");
                    return false;
                }
                IsNew = false;
                Title = "통장정보 수정";
                _bankBookNum = info.bank_book_num;

                Name = info.name ?? "";
                HolderName = info.owner ?? "";
                AccountNo = info.book_no ?? "";
                StartDate = info.start_date;
                EndDate = info.end_date;
                Note = info.note ?? "";
                SelectedBank = Banks.FirstOrDefault(x => x.SqCode == (int)info.bank_type);
                SelectOwner(info.owner_type);
                Images.Load(info.images);
            }

            IsNameFocused = true;
            return true;
        }

        protected override bool Validate(out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(Name))
            {
                error = "통장명을 입력하세요.";
                IsNameFocused = true;
                return false;
            }
            if (string.IsNullOrWhiteSpace(HolderName))
            {
                error = "예금주를 입력하세요.";
                return false;
            }
            return true;
        }

        protected override async Task<(bool Success, string Message)> SaveAsync()
        {
            var info = new BankBookInfo
            {
                bank_book_num = IsNew ? 0 : _bankBookNum,
                name = Name.Trim(),
                owner = HolderName.Trim(),
                bank_type = (CdBank)(SelectedBank?.SqCode ?? 0),
                owner_type = SelectedOwnerType,
                book_no = AccountNo.Trim(),
                start_date = StartDate,
                end_date = EndDate,
                note = Note.Trim(),
                status = CdStatus.사용,
                images = Images.ToList(), // 기존 + 신규(IsNew, data) 첨부. 신규는 매니저가 image/add 로 올린다.
            };

            var result = IsNew
                ? await _manager.InsertAsync(info)
                : await _manager.UpdateAsync(info, Images.DeletedImageIds);

            if (result.Success)
            {
                IsNew = false;
                Images.DeletedImageIds.Clear();
            }
            return result;
        }

        protected override void ResetForContinue()
        {
            _bankBookNum = 0;
            Name = "";
            HolderName = "";
            AccountNo = "";
            StartDate = null;
            EndDate = null;
            Note = "";
            SelectedBank = null;
            Images.Clear();
            Title = "통장정보 추가";
            IsNameFocused = true;
        }
    }
}
