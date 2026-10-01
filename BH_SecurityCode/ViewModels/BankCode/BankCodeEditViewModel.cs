using System.Collections.ObjectModel;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Api.Model;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using BH_SecurityCode.ViewModels.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using BH_SecurityCode.Api.Interface;

namespace BH_SecurityCode.ViewModels.BankCode
{
    /// <summary>보안카드 추가/수정 (기존 frmBankCodeInfo). API 응답 <see cref="BankCodeInfo"/> 기준으로 동작한다.</summary>
    public partial class BankCodeEditViewModel : EditViewModelBase
    {
        private readonly IBankCodeManager _manager;

        /// <summary>수정 대상 bank_code_num. 0 이면 신규</summary>
        private int _bankCodeNum;

        public ObservableCollection<CodeInfo> Banks { get; } = new();
        public BankCodeGridViewModel Grid { get; } = new();
        public ImageGridViewModel<BankCodeImageInfo> Images { get; }

        /// <summary>선택 가능한 보안카드 형식. API 코드는 항상 4자리이므로 4자리 형식만 제공한다.</summary>
        public IReadOnlyList<BankCodeCardTypes> CardTypes { get; } = Enum.GetValues<BankCodeCardTypes>()
            .Where(x => x != BankCodeCardTypes.None && (int)x / 100 == 4).ToList();

        [ObservableProperty] private string _name = "";
        [ObservableProperty] private CodeInfo? _selectedBank;
        [ObservableProperty] private string _serial = "";
        [ObservableProperty] private string _note = "";
        [ObservableProperty] private ViewTypes _viewType = ViewTypes.세로;
        [ObservableProperty] private BankCodeCardTypes _cardType = BankCodeCardTypes._4자리35개;
        [ObservableProperty] private bool _isNameFocused;

        public BankCodeEditViewModel(IBankCodeManager manager, ICodeManager codes, IDialogService dialog, IImageManager images)
            : base(dialog, codes)
        {
            _manager = manager;
            Images = new ImageGridViewModel<BankCodeImageInfo>(dialog, images, CdImageType.보안카드);
            ApplyCardType(CardType);
        }

        partial void OnViewTypeChanged(ViewTypes value) => Grid.ViewType = value;

        partial void OnCardTypeChanged(BankCodeCardTypes value) => ApplyCardType(value);

        private void ApplyCardType(BankCodeCardTypes value)
        {
            if (value == BankCodeCardTypes.None) return;
            Grid.MaxCodeLength = (int)value / 100;
            Grid.MaxCodeCount = (int)value % 100;
        }

        /// <summary>bankCodeNum 이 null/0 이면 추가, 아니면 상세 API 로 불러와 수정 모드로 연다.</summary>
        public async Task<bool> LoadAsync(int? bankCodeNum)
        {
            await LoadOwnersAsync();
            Banks.Clear();
            foreach (var code in await Codes.GetCodesAsync(CodeTypes.Bank))
                Banks.Add(code);

            if (bankCodeNum == null || bankCodeNum <= 0)
            {
                IsNew = true;
                Title = "보안카드정보 추가";
                _bankCodeNum = 0;
                Grid.Reset();
                Images.Clear();
            }
            else
            {
                var info = await _manager.GetDetailAsync(bankCodeNum.Value);
                if (info == null)
                {
                    Dialog.ShowError("보안카드정보를 불러오는데 실패 하였습니다.");
                    return false;
                }
                IsNew = false;
                Title = "보안카드정보 수정";
                _bankCodeNum = info.bank_code_num;

                Name = info.name ?? "";
                Serial = info.serial ?? "";
                Note = info.note ?? "";
                ViewType = info.code_align == CdAlign.가로 ? ViewTypes.가로 : ViewTypes.세로;
                var cardType = (BankCodeCardTypes)(400 + info.code_qty);
                CardType = CardTypes.Contains(cardType) ? cardType : BankCodeCardTypes._4자리35개;
                SelectedBank = Banks.FirstOrDefault(x => x.SqCode == (int)info.bank_type);
                SelectOwner((int)info.owner_type);
                Grid.LoadCodes(info.GetCodeMap());
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
                error = "보안카드명을 입력하세요.";
                IsNameFocused = true;
                return false;
            }
            if (SelectedBank == null)
            {
                error = "발급기관을 선택하세요.";
                return false;
            }
            if (SelectedOwner == null)
            {
                error = "소유자를 선택하세요.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Serial))
            {
                error = "일련번호를 입력하세요.";
                return false;
            }
            return true;
        }

        protected override async Task<(bool Success, string Message)> SaveAsync()
        {
            List<Dictionary<int, string>> lstCd = new List<Dictionary<int, string>>();
            foreach (var item in Grid.Codes)
            {
                if (string.IsNullOrEmpty(item.Code)) break;
                Dictionary<int, string> dic = new Dictionary<int, string>();
                dic.Add(item.No, item.Code);
                lstCd.Add(dic);
            }
            var info = new BankCodeInfo
            {
                bank_code_num = IsNew ? 0 : _bankCodeNum,
                name = Name.Trim(),
                bank_type = (CdBank)(SelectedBank?.SqCode ?? 0),
                owner_type = SelectedOwnerType,
                code_qty = Grid.MaxCodeCount,
                serial = Serial.Trim(),
                code_align = ViewType == ViewTypes.가로 ? CdAlign.가로 : CdAlign.세로,
                note = Note.Trim(),
                status = CdStatus.사용,
                codes = lstCd,
                images = Images.ToList(),
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
            _bankCodeNum = 0;
            Name = "";
            Serial = "";
            Note = "";
            SelectedBank = null;
            Grid.Reset();
            Images.Clear();
            Title = "보안카드정보 추가";
            IsNameFocused = true;
        }
    }
}
