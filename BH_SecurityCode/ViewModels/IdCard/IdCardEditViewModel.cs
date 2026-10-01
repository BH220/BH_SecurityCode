using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using BH_SecurityCode.ViewModels.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.ViewModels.IdCard
{
    /// <summary>
    /// 신분증 추가/수정 (기존 frmIdCardInfo). 신분증 종류(<see cref="CdIdType"/>)에 따라 입력 가능한 항목이 달라진다.
    /// API 응답 <see cref="IdCardInfo"/> 기준으로 동작한다.
    /// </summary>
    public partial class IdCardEditViewModel : EditViewModelBase
    {
        private readonly IIdCardManager _manager;

        /// <summary>수정 대상 id_num. 0 이면 신규</summary>
        private int _idNum;

        public ImageGridViewModel<IdCardImageInfo> Images { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IdTypeName))]
        [NotifyPropertyChangedFor(nameof(IsNameKorEnabled))]
        [NotifyPropertyChangedFor(nameof(IsNameEngEnabled))]
        [NotifyPropertyChangedFor(nameof(IsNameChaEnabled))]
        [NotifyPropertyChangedFor(nameof(IsPassportEnabled))]
        [NotifyPropertyChangedFor(nameof(IsLicenseEnabled))]
        [NotifyPropertyChangedFor(nameof(IsIssueOrgEnabled))]
        [NotifyPropertyChangedFor(nameof(IsExpireEnabled))]
        private CdIdType _idType = CdIdType.주민등록증;

        public string IdTypeName => IdType.ToString();

        // 신분증 종류별 입력 가능 항목 (기존 SetControl주민등록증/운전면허증/여권)
        public bool IsNameKorEnabled => IdType != CdIdType.여권;
        public bool IsNameEngEnabled => IdType == CdIdType.여권;
        public bool IsNameChaEnabled => IdType == CdIdType.주민등록증;
        public bool IsPassportEnabled => IdType == CdIdType.여권;
        public bool IsLicenseEnabled => IdType == CdIdType.운전면허증;
        public bool IsIssueOrgEnabled => IdType != CdIdType.여권;
        public bool IsExpireEnabled => IdType != CdIdType.주민등록증;

        [ObservableProperty] private string _alias = "";
        [ObservableProperty] private string _nameKor = "";
        [ObservableProperty] private string _nameEng = "";
        [ObservableProperty] private string _nameCha = "";
        [ObservableProperty] private string _idNo = "";
        /// <summary>여권번호 (API 필드 license_no 를 여권일 때 사용)</summary>
        [ObservableProperty] private string _passportNo = "";
        /// <summary>면허번호 (API 필드 license_no 를 운전면허증일 때 사용)</summary>
        [ObservableProperty] private string _licenseNo = "";
        [ObservableProperty] private DateTime? _issueDate;
        [ObservableProperty] private string _issueOrg = "";
        [ObservableProperty] private DateTime? _expireDate;
        [ObservableProperty] private string _address = "";
        [ObservableProperty] private string _addCondition = "";
        [ObservableProperty] private string _note = "";
        [ObservableProperty] private bool _isAliasFocused;

        public IdCardEditViewModel(IIdCardManager manager, ICodeManager codes, IDialogService dialog, IImageManager images)
            : base(dialog, codes)
        {
            _manager = manager;
            Images = new ImageGridViewModel<IdCardImageInfo>(dialog, images, CdImageType.신분증);
        }

        /// <summary>idNum 이 null/0 이면 추가, 아니면 상세 API 로 불러와 수정 모드로 연다.</summary>
        public async Task<bool> LoadAsync(int? idNum, CdIdType idType)
        {
            await LoadOwnersAsync();
            IdType = idType;

            if (idNum == null || idNum <= 0)
            {
                IsNew = true;
                Title = $"{IdTypeName}정보 추가";
                _idNum = 0;
                Images.Clear();
            }
            else
            {
                var info = await _manager.GetAsync(idNum.Value);
                if (info == null)
                {
                    Dialog.ShowError($"{IdTypeName} 정보를 불러오는데 실패하였습니다.");
                    return false;
                }
                IsNew = false;
                _idNum = info.id_num;
                IdType = info.id_type;
                Title = $"{IdTypeName}정보 수정";

                Alias = info.name ?? "";
                NameKor = info.name_kor ?? "";
                NameEng = info.name_eng ?? "";
                NameCha = info.name_cha ?? "";
                IdNo = info.id_no ?? "";
                PassportNo = IdType == CdIdType.여권 ? info.license_no ?? "" : "";
                LicenseNo = IdType == CdIdType.운전면허증 ? info.license_no ?? "" : "";
                IssueDate = info.issue_date;
                IssueOrg = info.license_org ?? "";
                ExpireDate = info.expired_date;
                Address = info.address ?? "";
                AddCondition = info.add_condition ?? "";
                Note = info.note ?? "";
                SelectOwner(info.owner);
                Images.Load(info.images);
            }

            IsAliasFocused = true;
            return true;
        }

        protected override bool Validate(out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(Alias))
            {
                error = "신분증 별칭을 입력하세요.";
                IsAliasFocused = true;
                return false;
            }
            return true;
        }

        protected override async Task<(bool Success, string Message)> SaveAsync()
        {
            var info = new IdCardInfo
            {
                id_num = IsNew ? 0 : _idNum,
                id_type = IdType,
                owner = SelectedOwnerType,
                name = Alias.Trim(),
                name_kor = IsNameKorEnabled ? NameKor.Trim() : "",
                name_eng = IsNameEngEnabled ? NameEng.Trim() : "",
                name_cha = IsNameChaEnabled ? NameCha.Trim() : "",
                id_no = IdNo.Trim(),
                // license_no 는 여권번호/면허번호 공용 필드
                license_no = IsPassportEnabled ? PassportNo.Trim() : IsLicenseEnabled ? LicenseNo.Trim() : "",
                license_org = IsIssueOrgEnabled ? IssueOrg.Trim() : "",
                issue_date = IssueDate,
                expired_date = IsExpireEnabled ? ExpireDate : null,
                address = Address.Trim(),
                add_condition = AddCondition.Trim(),
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
            _idNum = 0;
            Alias = "";
            NameKor = "";
            NameEng = "";
            NameCha = "";
            IdNo = "";
            PassportNo = "";
            LicenseNo = "";
            IssueDate = null;
            IssueOrg = "";
            ExpireDate = null;
            Address = "";
            AddCondition = "";
            Note = "";
            Images.Clear();
            Title = $"{IdTypeName}정보 추가";
            IsAliasFocused = true;
        }
    }
}
