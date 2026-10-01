using System.Collections.ObjectModel;
using System.ComponentModel;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Mobile.Models;
using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>
    /// 추가 · 수정 화면. 데스크톱은 종류별 모달 창(BankCodeEditWindow 등)이었지만
    /// 모바일에서는 입력 칸 목록 + 종류별 특수 영역(코드표 / 하위 계정 / 첨부)을 쌓는 한 화면으로 통일했다.
    ///
    /// 저장은 Manager 의 UpdateAsync 를 쓴다. 키가 0 이면 등록, 0 보다 크면 수정으로 서버가 처리한다.
    /// </summary>
    public sealed partial class VaultEditViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IVaultDataService _data;
        private readonly ICodeManager _codes;

        // 서버 모델 (수정이면 조회해 온 것, 추가면 새로 만든 것)
        private BankCodeInfo? _bankCode;
        private BankBookInfo? _bankBook;
        private CardInfo? _card;
        private IdCardInfo? _idCard;
        private AccountInfo? _account;

        private readonly List<int> _deleteImageIds = new();
        private readonly List<int> _deleteAccountIds = new();

        // 매핑에 쓰는 입력 칸 참조
        private EditFieldViewModel? _fName, _fOwner, _fNote;
        private EditFieldViewModel? _fBank, _fCodeQty, _fSerial, _fAlign;
        private EditFieldViewModel? _fBookOwner, _fBookNo, _fStart, _fEnd;
        private EditFieldViewModel? _fCardCompany, _fCardOwner, _fCardNo, _fExpire, _fCvc;
        private EditFieldViewModel? _fIdType, _fNameKor, _fNameEng, _fNameCha, _fIdNo, _fLicenseNo,
            _fLicenseOrg, _fIssue, _fExpired, _fAddress, _fAddCondition;
        private EditFieldViewModel? _fSiteAddress;

        public VaultEditViewModel(IVaultDataService data, ICodeManager codes)
        {
            _data = data;
            _codes = codes;
        }

        public ObservableCollection<EditFieldViewModel> Fields { get; } = new();
        public ObservableCollection<CodeEntryViewModel> Codes { get; } = new();
        public ObservableCollection<AccountEntryViewModel> Accounts { get; } = new();
        public ObservableCollection<EditAttachmentViewModel> Attachments { get; } = new();

        [ObservableProperty]
        private int _categoryId;

        [ObservableProperty]
        private int _itemKey;

        [ObservableProperty]
        private CategoryMeta _meta = Categories.Code;

        [ObservableProperty]
        private string _errorText = "";

        [ObservableProperty]
        private bool _isSaving;

        public bool IsNew => ItemKey == 0;
        public bool HasError => ErrorText.Length > 0;
        public bool HasCodes => Meta.Id == Menus.보안코드;
        public bool HasAccounts => Meta.Id == Menus.계정;
        public bool SupportsAttachments => Meta.Id != Menus.계정;
        public bool CanDelete => IsNew == false;

        public string PageTitle => IsNew ? $"{Meta.Title} 추가" : $"{Meta.Title} 수정";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("category", out object? category) &&
                int.TryParse(category?.ToString(), out int categoryId))
                CategoryId = categoryId;

            if (query.TryGetValue("key", out object? key) &&
                int.TryParse(key?.ToString(), out int itemKey))
                ItemKey = itemKey;

            _ = LoadAsync();
        }

        partial void OnCategoryIdChanged(int value)
        {
            Meta = Categories.Get((Menus)value);
            OnPropertyChanged(nameof(HasCodes));
            OnPropertyChanged(nameof(HasAccounts));
            OnPropertyChanged(nameof(SupportsAttachments));
            OnPropertyChanged(nameof(PageTitle));
        }

        partial void OnItemKeyChanged(int value)
        {
            OnPropertyChanged(nameof(IsNew));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(PageTitle));
        }

        partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(HasError));

        // ── 불러오기 ────────────────────────────────────────────────

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorText = "";
            Fields.Clear();
            Codes.Clear();
            Accounts.Clear();
            Attachments.Clear();
            _deleteImageIds.Clear();
            _deleteAccountIds.Clear();
            try
            {
                string message = Meta.Id switch
                {
                    Menus.보안코드 => await BuildCodeAsync(),
                    Menus.통장 => await BuildBookAsync(),
                    Menus.카드 => await BuildCardAsync(),
                    Menus.신분증 => await BuildIdAsync(),
                    Menus.계정 => await BuildAccountAsync(),
                    _ => "지원하지 않는 종류입니다.",
                };
                ErrorText = message;
                Title = PageTitle;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task RetryAsync() => LoadAsync();

        private async Task<IReadOnlyList<ChoiceOption>> OptionsAsync(CodeTypes type) =>
            (await _codes.GetCodesAsync(type))
                .Select(x => new ChoiceOption(x.SqCode, x.NmCode))
                .ToList();

        private static IReadOnlyList<ChoiceOption> EnumOptions<TEnum>() where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>()
                .Select(x => new ChoiceOption(Convert.ToInt32(x), x.ToString()!))
                .ToList();

        private EditFieldViewModel Add(EditFieldViewModel field)
        {
            Fields.Add(field);
            return field;
        }

        private EditFieldViewModel Text(string label, bool required = false, string placeholder = "",
            Keyboard? keyboard = null, int maxLength = 200) =>
            Add(new EditFieldViewModel
            {
                Label = label,
                Kind = EditKind.Text,
                Required = required,
                Placeholder = placeholder,
                Keyboard = keyboard ?? Keyboard.Default,
                MaxLength = maxLength,
            });

        private EditFieldViewModel Multiline(string label, string placeholder = "") =>
            Add(new EditFieldViewModel
            {
                Label = label,
                Kind = EditKind.Multiline,
                Placeholder = placeholder,
                MaxLength = 500,
            });

        private EditFieldViewModel Choice(string label, IReadOnlyList<ChoiceOption> options, int selected) =>
            Add(new EditFieldViewModel
            {
                Label = label,
                Kind = EditKind.Choice,
                Required = true,
                Options = options,
            }).Also(x => x.SetChoice(selected));

        private EditFieldViewModel DateField(string label, DateTime? value, bool optional) =>
            Add(new EditFieldViewModel
            {
                Label = label,
                Kind = EditKind.Date,
                DateOptional = optional,
            }).Also(x => x.SetDate(value));

        // ── 종류별 화면 구성 ────────────────────────────────────────

        private async Task<string> BuildCodeAsync()
        {
            if (IsNew)
            {
                _bankCode = new BankCodeInfo { name = "", serial = "", note = "", code_qty = 30 };
            }
            else
            {
                var result = await _data.GetSecurityCodeAsync(ItemKey);
                if (result.Success == false || result.Data == null)
                    return result.Message;
                _bankCode = result.Data;
            }

            _fName = Text("보안코드명", required: true, placeholder: "예) 국민 주거래 보안카드");
            _fBank = Choice("발급기관", await OptionsAsync(CodeTypes.Bank), (int)_bankCode.bank_type);
            _fOwner = Choice("소유자", await OptionsAsync(CodeTypes.Person), (int)_bankCode.owner_type);
            _fSerial = Text("일련번호", required: true, placeholder: "카드에 인쇄된 번호");
            _fCodeQty = Text("코드 수", required: true, placeholder: "30", keyboard: Keyboard.Numeric, maxLength: 3);
            _fAlign = Choice("배열 방향", EnumOptions<CdAlign>(), (int)_bankCode.code_align);
            _fNote = Multiline("비고");

            _fName.Text = _bankCode.name ?? "";
            _fSerial.Text = _bankCode.serial ?? "";
            _fCodeQty.Text = _bankCode.code_qty > 0 ? _bankCode.code_qty.ToString() : "30";
            _fNote.Text = _bankCode.note ?? "";

            // 코드 수를 바꾸면 입력 칸을 다시 만든다. (입력해 둔 값은 살린다)
            _fCodeQty.PropertyChanged += OnCodeQtyChanged;

            var map = _bankCode.GetCodeMap();
            RebuildCodes(_bankCode.code_qty > 0 ? _bankCode.code_qty : 30, map);
            AddAttachments(_bankCode.images);
            return "";
        }

        private void OnCodeQtyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(EditFieldViewModel.Text))
                return;

            if (int.TryParse(_fCodeQty?.Text, out int qty) == false)
                return;

            RebuildCodes(qty, Codes.ToDictionary(x => x.No, x => x.Code));
        }

        private void RebuildCodes(int qty, IReadOnlyDictionary<int, string> existing)
        {
            qty = Math.Clamp(qty, 0, 127);
            if (Codes.Count == qty)
                return;

            Codes.Clear();
            for (int no = 1; no <= qty; no++)
                Codes.Add(new CodeEntryViewModel(no, existing.TryGetValue(no, out string? code) ? code : ""));
        }

        private async Task<string> BuildBookAsync()
        {
            if (IsNew)
            {
                _bankBook = new BankBookInfo { name = "", owner = "", book_no = "", note = "" };
            }
            else
            {
                var result = await _data.GetBankBookAsync(ItemKey);
                if (result.Success == false || result.Data == null)
                    return result.Message;
                _bankBook = result.Data;
            }

            _fName = Text("통장명", required: true, placeholder: "예) 주거래 입출금");
            _fBank = Choice("은행", await OptionsAsync(CodeTypes.Bank), (int)_bankBook.bank_type);
            _fOwner = Choice("소유자", await OptionsAsync(CodeTypes.Person), (int)_bankBook.owner_type);
            _fBookOwner = Text("예금주", required: true);
            _fBookNo = Text("계좌번호", required: true, placeholder: "숫자와 - 만", keyboard: Keyboard.Numeric, maxLength: 30);
            _fStart = DateField("개설일자", _bankBook.start_date, optional: true);
            _fEnd = DateField("만기일자", _bankBook.end_date, optional: true);
            _fNote = Multiline("비고");

            _fName.Text = _bankBook.name ?? "";
            _fBookOwner.Text = _bankBook.owner ?? "";
            _fBookNo.Text = _bankBook.book_no ?? "";
            _fNote.Text = _bankBook.note ?? "";

            AddAttachments(_bankBook.images);
            return "";
        }

        private async Task<string> BuildCardAsync()
        {
            if (IsNew)
            {
                _card = new CardInfo { name = "", card_no = "", owner_name = "", expire_date = "", cvc = "", note = "" };
            }
            else
            {
                var result = await _data.GetCardAsync(ItemKey);
                if (result.Success == false || result.Data == null)
                    return result.Message;
                _card = result.Data;
            }

            _fName = Text("카드명", required: true, placeholder: "예) 주거래 체크");
            _fCardCompany = Choice("카드사", await OptionsAsync(CodeTypes.Card), (int)_card.card_type);
            _fOwner = Choice("소유자", await OptionsAsync(CodeTypes.Person), (int)_card.owner);
            _fCardOwner = Text("명의자", required: true);
            _fCardNo = Text("카드번호", required: true, placeholder: "16자리", keyboard: Keyboard.Numeric, maxLength: 25);
            _fExpire = Text("유효기간", required: true, placeholder: "202812 (yyyyMM)", keyboard: Keyboard.Numeric, maxLength: 6);
            _fCvc = Text("CVC", required: true, placeholder: "3자리", keyboard: Keyboard.Numeric, maxLength: 4);
            _fNote = Multiline("비고");

            _fName.Text = _card.name ?? "";
            _fCardOwner.Text = _card.owner_name ?? "";
            _fCardNo.Text = _card.card_no ?? "";
            _fExpire.Text = _card.expire_date ?? "";
            _fCvc.Text = _card.cvc ?? "";
            _fNote.Text = _card.note ?? "";

            AddAttachments(_card.images);
            return "";
        }

        private async Task<string> BuildIdAsync()
        {
            if (IsNew)
            {
                _idCard = new IdCardInfo
                {
                    name = "", name_kor = "", name_eng = "", name_cha = "", id_no = "",
                    license_no = "", license_org = "", address = "", add_condition = "", note = "",
                };
            }
            else
            {
                var result = await _data.GetIdCardAsync(ItemKey);
                if (result.Success == false || result.Data == null)
                    return result.Message;
                _idCard = result.Data;
            }

            _fName = Text("신분증 별칭", required: true, placeholder: "예) 본인 주민등록증");
            _fIdType = Choice("종류", await OptionsAsync(CodeTypes.ID_Card), (int)_idCard.id_type);
            _fOwner = Choice("소유자", await OptionsAsync(CodeTypes.Person), (int)_idCard.owner);
            _fNameKor = Text("이름");
            _fNameEng = Text("영문 이름");
            _fNameCha = Text("한자 이름");
            _fIdNo = Text("주민등록번호", required: true, placeholder: "000000-0000000", maxLength: 20);
            _fLicenseNo = Text("면허 / 여권번호", maxLength: 30);
            _fLicenseOrg = Text("발급기관");
            _fIssue = DateField("발급일", _idCard.issue_date, optional: true);
            _fExpired = DateField("만료일", _idCard.expired_date, optional: true);
            _fAddCondition = Text("추가기재사항");
            _fAddress = Multiline("주소");
            _fNote = Multiline("비고");

            _fName.Text = _idCard.name ?? "";
            _fNameKor.Text = _idCard.name_kor ?? "";
            _fNameEng.Text = _idCard.name_eng ?? "";
            _fNameCha.Text = _idCard.name_cha ?? "";
            _fIdNo.Text = _idCard.id_no ?? "";
            _fLicenseNo.Text = _idCard.license_no ?? "";
            _fLicenseOrg.Text = _idCard.license_org ?? "";
            _fAddCondition.Text = _idCard.add_condition ?? "";
            _fAddress.Text = _idCard.address ?? "";
            _fNote.Text = _idCard.note ?? "";

            AddAttachments(_idCard.images);
            return "";
        }

        private async Task<string> BuildAccountAsync()
        {
            if (IsNew)
            {
                _account = new AccountInfo { name = "", address = "", note = "" };
            }
            else
            {
                var result = await _data.GetAccountAsync(ItemKey);
                if (result.Success == false || result.Data == null)
                    return result.Message;
                _account = result.Data;
            }

            _fName = Text("사이트명", required: true, placeholder: "예) 국민은행 인터넷뱅킹");
            _fOwner = Choice("소유자", await OptionsAsync(CodeTypes.Person), (int)_account.owner);
            _fSiteAddress = Text("주소", required: true, placeholder: "banking.kbstar.com", keyboard: Keyboard.Url);
            _fNote = Multiline("비고");

            _fName.Text = _account.name ?? "";
            _fSiteAddress.Text = _account.address ?? "";
            _fNote.Text = _account.note ?? "";

            foreach (var item in _account.items.Where(x => x.status == CdStatus.사용))
                Accounts.Add(new AccountEntryViewModel(item, RemoveAccount));

            if (Accounts.Count == 0)
                Accounts.Add(new AccountEntryViewModel(null, RemoveAccount));

            return "";
        }

        // ── 하위 계정 ──────────────────────────────────────────────

        [RelayCommand]
        private void AddAccount() => Accounts.Add(new AccountEntryViewModel(null, RemoveAccount));

        private void RemoveAccount(AccountEntryViewModel entry)
        {
            if (entry.DetailNum > 0)
                _deleteAccountIds.Add(entry.DetailNum);

            Accounts.Remove(entry);
        }

        // ── 첨부 ───────────────────────────────────────────────────

        private void AddAttachments(IEnumerable<ImageInfo> images)
        {
            foreach (var image in images)
                Attachments.Add(new EditAttachmentViewModel(image, RemoveAttachment));
        }

        private void RemoveAttachment(EditAttachmentViewModel item)
        {
            if (item.IsNew == false)
                _deleteImageIds.Add(item.Image.LinkImageNum);

            Attachments.Remove(item);
        }

        [RelayCommand]
        private Task PickPhotoAsync() => AddPhotoAsync(capture: false);

        [RelayCommand]
        private Task CapturePhotoAsync() => AddPhotoAsync(capture: true);

        private async Task AddPhotoAsync(bool capture)
        {
            try
            {
                if (capture && MediaPicker.Default.IsCaptureSupported == false)
                {
                    ShowToast("이 기기에서는 촬영을 지원하지 않습니다");
                    return;
                }

                FileResult? file = capture
                    ? await MediaPicker.Default.CapturePhotoAsync()
                    : await MediaPicker.Default.PickPhotoAsync();

                if (file == null)
                    return;

                using var stream = await file.OpenReadAsync();
                using var buffer = new MemoryStream();
                await stream.CopyToAsync(buffer);
                byte[] data = buffer.ToArray();

                if (data.Length == 0)
                {
                    ShowToast("이미지를 읽지 못했습니다");
                    return;
                }

                var image = NewImage(file.FileName, data);
                var preview = ImageSource.FromStream(() => new MemoryStream(data));
                Attachments.Add(new EditAttachmentViewModel(image, RemoveAttachment, preview));
            }
            catch (PermissionException)
            {
                ShowToast("사진 접근 권한이 필요합니다");
            }
            catch (Exception ex)
            {
                ShowToast(ex.Message);
            }
        }

        /// <summary>종류에 맞는 첨부 모델을 만든다. LinkImageNum 매핑이 종류별로 다르다.</summary>
        private ImageInfo NewImage(string fileName, byte[] data)
        {
            string name = string.IsNullOrWhiteSpace(fileName) ? $"image_{DateTime.Now:yyyyMMddHHmmss}.jpg" : fileName;
            string extension = Path.GetExtension(name).TrimStart('.').ToLowerInvariant();

            ImageInfo image = Meta.Id switch
            {
                Menus.보안코드 => new BankCodeImageInfo(),
                Menus.통장 => new BankBookImageInfo(),
                Menus.카드 => new CardImageInfo(),
                Menus.신분증 => new IdCardImageInfo(),
                _ => new ImageInfo(),
            };

            image.name = name;
            image.extension = extension;
            image.size = data.Length;
            image.data = data;
            image.status = CdStatus.사용;
            image.type = Meta.Id switch
            {
                Menus.보안코드 => CdImageType.보안카드,
                Menus.통장 => CdImageType.통장,
                Menus.카드 => CdImageType.카드,
                Menus.신분증 => CdImageType.신분증,
                _ => CdImageType.보안카드,
            };
            return image;
        }

        // ── 저장 ───────────────────────────────────────────────────

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsSaving)
                return;

            if (Validate() == false)
                return;

            IsSaving = true;
            ErrorText = "";
            try
            {
                var result = Meta.Id switch
                {
                    Menus.보안코드 => await SaveCodeAsync(),
                    Menus.통장 => await SaveBookAsync(),
                    Menus.카드 => await SaveCardAsync(),
                    Menus.신분증 => await SaveIdAsync(),
                    Menus.계정 => await SaveAccountAsync(),
                    _ => VaultResult<bool>.Fail("지원하지 않는 종류입니다."),
                };

                if (result.Success == false)
                {
                    ErrorText = result.Message;
                    return;
                }

                // 이미지 일부 실패처럼 부분 경고가 있으면 알려준다.
                if (string.IsNullOrWhiteSpace(result.Message) == false)
                    await AlertAsync("저장했습니다", result.Message);

                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsSaving = false;
            }
        }

        private Task<VaultResult<bool>> SaveCodeAsync()
        {
            _bankCode!.name = _fName!.Text.Trim();
            _bankCode.bank_type = (CdBank)_fBank!.ChoiceValue;
            _bankCode.owner_type = (CdOwner)_fOwner!.ChoiceValue;
            _bankCode.serial = _fSerial!.Text.Trim();
            _bankCode.code_align = (CdAlign)_fAlign!.ChoiceValue;
            _bankCode.code_qty = int.TryParse(_fCodeQty!.Text, out int qty) ? qty : 0;
            _bankCode.note = _fNote!.Text.Trim();

            // 서버 형식: code_values = "1:1234,2:5678". 빈 칸은 보내지 않는다.
            _bankCode.codes = Codes
                .Where(x => string.IsNullOrWhiteSpace(x.Code) == false)
                .Select(x => new Dictionary<int, string> { [x.No] = x.Code.Trim() })
                .ToList();

            return _data.SaveSecurityCodeAsync(_bankCode, _deleteImageIds);
        }

        private Task<VaultResult<bool>> SaveBookAsync()
        {
            _bankBook!.name = _fName!.Text.Trim();
            _bankBook.bank_type = (CdBank)_fBank!.ChoiceValue;
            _bankBook.owner_type = (CdOwner)_fOwner!.ChoiceValue;
            _bankBook.owner = _fBookOwner!.Text.Trim();
            _bankBook.book_no = _fBookNo!.Text.Trim();
            _bankBook.start_date = _fStart!.DateValue;
            _bankBook.end_date = _fEnd!.DateValue;
            _bankBook.note = _fNote!.Text.Trim();

            return _data.SaveBankBookAsync(_bankBook, _deleteImageIds);
        }

        private Task<VaultResult<bool>> SaveCardAsync()
        {
            _card!.name = _fName!.Text.Trim();
            _card.card_type = (CdCardCompany)_fCardCompany!.ChoiceValue;
            _card.owner = (CdOwner)_fOwner!.ChoiceValue;
            _card.owner_name = _fCardOwner!.Text.Trim();
            _card.card_no = _fCardNo!.Text.Trim();
            _card.expire_date = _fExpire!.Text.Trim();
            _card.cvc = _fCvc!.Text.Trim();
            _card.note = _fNote!.Text.Trim();

            return _data.SaveCardAsync(_card, _deleteImageIds);
        }

        private Task<VaultResult<bool>> SaveIdAsync()
        {
            _idCard!.name = _fName!.Text.Trim();
            _idCard.id_type = (CdIdType)_fIdType!.ChoiceValue;
            _idCard.owner = (CdOwner)_fOwner!.ChoiceValue;
            _idCard.name_kor = _fNameKor!.Text.Trim();
            _idCard.name_eng = _fNameEng!.Text.Trim();
            _idCard.name_cha = _fNameCha!.Text.Trim();
            _idCard.id_no = _fIdNo!.Text.Trim();
            _idCard.license_no = _fLicenseNo!.Text.Trim();
            _idCard.license_org = _fLicenseOrg!.Text.Trim();
            _idCard.issue_date = _fIssue!.DateValue;
            _idCard.expired_date = _fExpired!.DateValue;
            _idCard.address = _fAddress!.Text.Trim();
            _idCard.add_condition = _fAddCondition!.Text.Trim();
            _idCard.note = _fNote!.Text.Trim();

            return _data.SaveIdCardAsync(_idCard, _deleteImageIds);
        }

        private Task<VaultResult<bool>> SaveAccountAsync()
        {
            _account!.name = _fName!.Text.Trim();
            _account.owner = (CdOwner)_fOwner!.ChoiceValue;
            _account.address = _fSiteAddress!.Text.Trim();
            _account.note = _fNote!.Text.Trim();

            // 비어있지 않은 줄만 보낸다. IsNewOrUpdate 를 세워야 Manager 가 sub/update 를 호출한다.
            _account.items = Accounts
                .Where(x => x.IsEmpty == false)
                .Select(x => new AccountItemInfo
                {
                    account_detail_num = x.DetailNum,
                    account_num = _account!.account_num,
                    id = x.Id.Trim(),
                    pw = x.Password,
                    status = CdStatus.사용,
                    IsNewOrUpdate = true,
                })
                .ToList();

            return _data.SaveAccountAsync(_account, _deleteAccountIds);
        }

        private bool Validate()
        {
            ErrorText = "";
            bool ok = true;

            foreach (var field in Fields)
            {
                field.Error = "";
                if (field.Required && field.Kind == EditKind.Text && string.IsNullOrWhiteSpace(field.Text))
                {
                    field.Error = $"{field.Label}을(를) 입력하세요.";
                    ok = false;
                }
                if (field.Required && field.Kind == EditKind.Choice && field.Choice == null)
                {
                    field.Error = $"{field.Label}을(를) 선택하세요.";
                    ok = false;
                }
            }

            if (Meta.Id == Menus.보안코드)
            {
                if (int.TryParse(_fCodeQty?.Text, out int qty) == false || qty <= 0 || qty > 127)
                {
                    _fCodeQty!.Error = "코드 수는 1~127 사이 숫자입니다.";
                    ok = false;
                }
            }

            if (Meta.Id == Menus.카드)
            {
                string expire = _fExpire?.Text.Trim() ?? "";
                if (expire.Length > 0 && (expire.Length != 6 || expire.All(char.IsDigit) == false))
                {
                    _fExpire!.Error = "유효기간은 yyyyMM 형식 6자리입니다.";
                    ok = false;
                }
            }

            if (Meta.Id == Menus.계정 && Accounts.All(x => x.IsEmpty))
            {
                ErrorText = "아이디와 비밀번호를 한 줄 이상 입력하세요.";
                ok = false;
            }

            if (ok == false && ErrorText.Length == 0)
                ErrorText = "입력을 확인하세요.";

            return ok;
        }

        // ── 삭제 ───────────────────────────────────────────────────

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (IsNew || IsSaving)
                return;

            bool confirm = await ConfirmAsync(
                $"{Meta.Title} 삭제",
                $"'{_fName?.Text}' 을(를) 삭제할까요?\n서버에서는 사용불가 처리됩니다.",
                "삭제");

            if (confirm == false)
                return;

            IsSaving = true;
            try
            {
                var result = await _data.DeleteAsync(Meta.Id, ItemKey);
                if (result.Success == false)
                {
                    ErrorText = result.Message;
                    return;
                }

                // 목록까지 한 번에 돌아간다. (상세 화면은 이미 지워진 항목을 보여주게 되므로)
                await Shell.Current.GoToAsync("../..");
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

        private static Page? CurrentPage => Application.Current?.Windows.FirstOrDefault()?.Page;

        private static async Task<bool> ConfirmAsync(string title, string message, string accept)
        {
            var page = CurrentPage;
            if (page == null)
                return false;

            return await page.DisplayAlert(title, message, accept, "취소");
        }

        private static async Task AlertAsync(string title, string message)
        {
            var page = CurrentPage;
            if (page != null)
                await page.DisplayAlert(title, message, "확인");
        }
    }

    internal static class FluentExtensions
    {
        /// <summary>생성 직후 초기화를 이어서 하기 위한 도우미.</summary>
        public static T Also<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }
    }
}
