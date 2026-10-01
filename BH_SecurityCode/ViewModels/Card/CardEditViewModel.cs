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

namespace BH_SecurityCode.ViewModels.Card
{
    /// <summary>카드 추가/수정 (기존 frmCardInfo). API 응답 <see cref="CardInfo"/> 기준으로 동작한다.</summary>
    public partial class CardEditViewModel : EditViewModelBase
    {
        private readonly ICardManager _manager;

        /// <summary>수정 대상 card_num. 0 이면 신규</summary>
        private int _cardNum;

        public ObservableCollection<CodeInfo> Companies { get; } = new();
        public ImageGridViewModel<CardImageInfo> Images { get; }

        [ObservableProperty] private string _name = "";
        [ObservableProperty] private CodeInfo? _selectedCompany;
        [ObservableProperty] private string _holderName = "";
        [ObservableProperty] private string _cardNo = "";
        [ObservableProperty] private DateTime? _expireDate;
        [ObservableProperty] private string _cvc = "";
        [ObservableProperty] private string _note = "";
        [ObservableProperty] private bool _isNameFocused;

        public CardEditViewModel(ICardManager manager, ICodeManager codes, IDialogService dialog, IImageManager images)
            : base(dialog, codes)
        {
            _manager = manager;
            Images = new ImageGridViewModel<CardImageInfo>(dialog, images, CdImageType.카드);
        }

        /// <summary>cardNum 이 null/0 이면 추가, 아니면 상세 API 로 불러와 수정 모드로 연다.</summary>
        public async Task<bool> LoadAsync(int? cardNum)
        {
            await LoadOwnersAsync();
            Companies.Clear();
            foreach (var code in await Codes.GetCodesAsync(CodeTypes.Card))
                Companies.Add(code);

            if (cardNum == null || cardNum <= 0)
            {
                IsNew = true;
                Title = "카드정보 추가";
                _cardNum = 0;
                Images.Clear();
            }
            else
            {
                var info = await _manager.GetAsync(cardNum.Value);
                if (info == null)
                {
                    Dialog.ShowError("카드정보를 불러오는데 실패 하였습니다.");
                    return false;
                }
                IsNew = false;
                Title = "카드정보 수정";
                _cardNum = info.card_num;

                Name = info.name ?? "";
                HolderName = info.owner_name ?? "";
                CardNo = info.card_no ?? "";
                ExpireDate = info.ExpireDate;
                Cvc = info.cvc ?? "";
                Note = info.note ?? "";
                SelectedCompany = Companies.FirstOrDefault(x => x.SqCode == (int)info.card_type);
                SelectOwner(info.owner);
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
                error = "카드명을 입력하세요.";
                IsNameFocused = true;
                return false;
            }
            if (string.IsNullOrWhiteSpace(CardNo))
            {
                error = "카드번호를 입력하세요.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Cvc))
            {
                error = "CVC번호를 입력하세요.";
                return false;
            }
            return true;
        }

        protected override async Task<(bool Success, string Message)> SaveAsync()
        {
            var info = new CardInfo
            {
                card_num = IsNew ? 0 : _cardNum,
                name = Name.Trim(),
                card_no = CardNo.Trim(),
                owner = SelectedOwnerType,
                card_type = (CdCardCompany)(SelectedCompany?.SqCode ?? 0),
                owner_name = HolderName.Trim(),
                ExpireDate = ExpireDate, // expire_date(yyyyMM) 로 저장된다
                cvc = Cvc.Trim(),
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
            _cardNum = 0;
            Name = "";
            HolderName = "";
            CardNo = "";
            ExpireDate = null;
            Cvc = "";
            Note = "";
            SelectedCompany = null;
            Images.Clear();
            Title = "카드정보 추가";
            IsNameFocused = true;
        }
    }
}
