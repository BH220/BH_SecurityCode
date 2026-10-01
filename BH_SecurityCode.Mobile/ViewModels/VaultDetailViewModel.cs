using System.Collections.ObjectModel;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Mobile.Models;
using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>상세 화면의 한 줄. 민감한 값은 가려서 넣고 눈 아이콘으로 연다.</summary>
    public sealed partial class FieldViewModel : ObservableObject
    {
        private readonly Action<string> _toast;
        private readonly string _raw;

        public FieldViewModel(string label, string? value, Action<string> toast,
            bool sensitive = false, bool mono = false, bool multiline = false, bool maskAll = false)
        {
            Label = label;
            _raw = value ?? "";
            _toast = toast;
            IsSensitive = sensitive && _raw.Length > 0;
            IsMono = mono;
            IsMultiline = multiline;
            MaskAll = maskAll;
            _isRevealed = IsSensitive == false;
        }

        public string Label { get; }
        public bool IsSensitive { get; }
        public bool IsMono { get; }
        public bool IsMultiline { get; }
        public bool MaskAll { get; }
        public bool HasValue => _raw.Length > 0;

        /// <summary>복사 버튼을 보일지. "30개", "가로" 같은 값은 복사할 이유가 없다.</summary>
        public bool CanCopy => HasValue && (IsSensitive || IsMono || IsMultiline);

        /// <summary>묶음 안에서 마지막 줄이 아니면 아래에 구분선을 그린다.</summary>
        public bool ShowDivider { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Value))]
        private bool _isRevealed;

        public string Value
        {
            get
            {
                if (_raw.Length == 0)
                    return "-";
                if (IsRevealed)
                    return _raw;
                return MaskAll ? Mask.All(_raw) : Mask.Tail(_raw);
            }
        }

        [RelayCommand]
        private void ToggleReveal() => IsRevealed = !IsRevealed;

        [RelayCommand]
        private async Task CopyAsync()
        {
            if (HasValue == false)
                return;

            await Clipboard.SetTextAsync(_raw);
            _toast($"{Label} 복사");
        }
    }

    /// <summary>상세 화면의 묶음(카드 한 장).</summary>
    public sealed class FieldGroupViewModel
    {
        public required string Title { get; init; }
        public required IReadOnlyList<FieldViewModel> Fields { get; init; }
    }

    /// <summary>첨부 이미지 한 장. 목록에서는 파일명만 보여주고, 누르면 서버에서 받아 펼친다.</summary>
    public sealed partial class AttachmentViewModel : ObservableObject
    {
        private readonly IVaultDataService _data;
        private readonly Action<string> _toast;

        public AttachmentViewModel(ImageInfo image, IVaultDataService data, Action<string> toast)
        {
            Image = image;
            _data = data;
            _toast = toast;
        }

        public ImageInfo Image { get; }
        public string FileName => Image.FileName;
        public string SizeText => Image.SizeText;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreview))]
        private ImageSource? _preview;

        public bool HasPreview => Preview != null;

        [RelayCommand]
        private async Task ToggleAsync()
        {
            if (HasPreview)
            {
                Preview = null;
                return;
            }

            if (IsLoading)
                return;

            IsLoading = true;
            try
            {
                var result = await _data.LoadImageAsync(Image);
                if (result.Success == false || result.Data == null)
                {
                    _toast(result.Message);
                    return;
                }

                byte[] bytes = result.Data;
                Preview = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>
    /// 항목 상세. 데스크톱은 종류별 모달 창(BankCodeEditWindow 등)이었지만
    /// 모바일에서는 필드 묶음을 위에서 아래로 쌓는 한 가지 화면으로 통일했다.
    /// </summary>
    public sealed partial class VaultDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IVaultDataService _data;

        public VaultDetailViewModel(IVaultDataService data)
        {
            _data = data;
        }

        public ObservableCollection<FieldGroupViewModel> Groups { get; } = new();
        public ObservableCollection<AttachmentViewModel> Attachments { get; } = new();

        [ObservableProperty]
        private int _categoryId;

        [ObservableProperty]
        private int _itemKey;

        [ObservableProperty]
        private CategoryMeta _meta = Categories.Code;

        [ObservableProperty]
        private string _headline = "";

        [ObservableProperty]
        private string _subline = "";

        [ObservableProperty]
        private string _ownerName = "";

        [ObservableProperty]
        private string _note = "";

        [ObservableProperty]
        private string _errorText = "";

        /// <summary>보안코드일 때만 코드표 버튼을 보여준다.</summary>
        [ObservableProperty]
        private bool _hasCodeTable;

        [ObservableProperty]
        private int _codeQty;

        public bool HasError => ErrorText.Length > 0;
        public bool HasNote => string.IsNullOrWhiteSpace(Note) == false;
        public bool HasAttachments => Attachments.Count > 0;

        /// <summary>Shell 이 넘겨준 detail?category=101&amp;key=3 을 받는다. category 를 먼저 반영해야 한다.</summary>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("category", out object? category) &&
                int.TryParse(category?.ToString(), out int categoryId))
                CategoryId = categoryId;

            if (query.TryGetValue("key", out object? key) &&
                int.TryParse(key?.ToString(), out int itemKey))
                ItemKey = itemKey;
        }

        partial void OnCategoryIdChanged(int value) => Meta = Categories.Get((Menus)value);

        partial void OnItemKeyChanged(int value) => _ = LoadAsync();

        partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(HasError));

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy || ItemKey == 0)
                return;

            IsBusy = true;
            ErrorText = "";
            Groups.Clear();
            Attachments.Clear();
            try
            {
                string message = Meta.Id switch
                {
                    Menus.보안코드 => await LoadCodeAsync(),
                    Menus.통장 => await LoadBookAsync(),
                    Menus.카드 => await LoadCardAsync(),
                    Menus.신분증 => await LoadIdAsync(),
                    Menus.계정 => await LoadAccountAsync(),
                    _ => "",
                };

                ErrorText = message;
                if (message.Length > 0 && Headline.Length == 0)
                    Headline = Meta.Title; // 헤더가 비어 보이지 않게

                Title = Headline;
                OnPropertyChanged(nameof(HasNote));
                OnPropertyChanged(nameof(HasAttachments));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task RetryAsync() => LoadAsync();

        /// <summary>값이 없는 줄은 아예 넣지 않는다. 화면에 "-" 만 늘어놓지 않기 위함.</summary>
        private void AddGroup(string title, params FieldViewModel[] fields)
        {
            var visible = fields.Where(x => x.HasValue).ToList();
            if (visible.Count == 0)
                return;

            for (int idx = 0; idx < visible.Count - 1; idx++)
                visible[idx].ShowDivider = true;

            Groups.Add(new FieldGroupViewModel { Title = title, Fields = visible });
        }

        private FieldViewModel F(string label, string? value, bool sensitive = false,
            bool mono = false, bool multiline = false, bool maskAll = false) =>
            new(label, value, ShowToast, sensitive, mono, multiline, maskAll);

        private static string D(DateTime? value) => value?.ToString("yyyy.MM.dd") ?? "";

        private void AddAttachments(IEnumerable<ImageInfo> images)
        {
            foreach (var image in images)
                Attachments.Add(new AttachmentViewModel(image, _data, ShowToast));
        }

        private async Task<string> LoadCodeAsync()
        {
            var result = await _data.GetSecurityCodeAsync(ItemKey);
            if (result.Success == false || result.Data == null)
                return result.Message;

            var item = result.Data;
            Headline = item.name ?? "";
            Subline = $"{item.bank_type}은행 보안카드";
            OwnerName = item.owner_type.ToString();
            Note = item.note ?? "";
            CodeQty = item.code_qty;
            HasCodeTable = item.GetCodeMap().Count > 0;

            AddGroup("카드 정보",
                F("일련번호", item.serial, sensitive: true, mono: true),
                F("발급기관", $"{item.bank_type}은행"),
                F("코드 수", item.code_qty > 0 ? $"{item.code_qty}개" : ""),
                F("배열 방향", item.code_align.ToString()));

            AddAttachments(item.images);
            return "";
        }

        private async Task<string> LoadBookAsync()
        {
            var result = await _data.GetBankBookAsync(ItemKey);
            if (result.Success == false || result.Data == null)
                return result.Message;

            var item = result.Data;
            Headline = item.name ?? "";
            Subline = $"{item.bank_type}은행";
            OwnerName = item.owner_type.ToString();
            Note = item.note ?? "";

            AddGroup("계좌",
                F("계좌번호", item.book_no, sensitive: true, mono: true),
                F("예금주", item.owner),
                F("은행", $"{item.bank_type}은행"));

            AddGroup("기간",
                F("개설일자", D(item.start_date)),
                F("만기일자", D(item.end_date)));

            AddAttachments(item.images);
            return "";
        }

        private async Task<string> LoadCardAsync()
        {
            var result = await _data.GetCardAsync(ItemKey);
            if (result.Success == false || result.Data == null)
                return result.Message;

            var item = result.Data;
            Headline = item.name ?? "";
            Subline = $"{item.card_type}카드";
            OwnerName = item.owner.ToString();
            Note = item.note ?? "";

            AddGroup("카드",
                F("카드번호", item.card_no, sensitive: true, mono: true),
                F("명의자", item.owner_name),
                F("카드사", $"{item.card_type}"));

            AddGroup("인증",
                F("유효기간", VaultItem.ExpireText(item.expire_date), mono: true),
                F("CVC", item.cvc, sensitive: true, mono: true, maskAll: true));

            AddAttachments(item.images);
            return "";
        }

        private async Task<string> LoadIdAsync()
        {
            var result = await _data.GetIdCardAsync(ItemKey);
            if (result.Success == false || result.Data == null)
                return result.Message;

            var item = result.Data;
            Headline = item.name ?? "";
            Subline = item.id_type.ToString();
            OwnerName = item.owner.ToString();
            Note = item.note ?? "";

            AddGroup("본인 확인",
                F("이름", item.name_kor),
                F("영문 이름", item.name_eng),
                F("한자 이름", item.name_cha),
                F("주민등록번호", item.id_no, sensitive: true, mono: true),
                F(item.id_type == CdIdType.여권 ? "여권번호" : "면허번호", item.license_no, sensitive: true, mono: true));

            AddGroup("발급",
                F("발급기관", item.license_org),
                F("발급일", D(item.issue_date)),
                F("만료일", D(item.expired_date)),
                F("추가기재사항", item.add_condition));

            AddGroup("주소",
                F("주소", item.address, multiline: true));

            AddAttachments(item.images);
            return "";
        }

        private async Task<string> LoadAccountAsync()
        {
            var result = await _data.GetAccountAsync(ItemKey);
            if (result.Success == false || result.Data == null)
                return result.Message;

            var item = result.Data;
            Headline = item.name ?? "";
            Subline = string.IsNullOrWhiteSpace(item.address) ? "계정" : item.address;
            OwnerName = item.owner.ToString();
            Note = item.note ?? "";

            AddGroup("사이트",
                F("사이트명", item.name),
                F("주소", item.address));

            var accounts = item.items.Where(x => x.status == CdStatus.사용).ToList();
            int no = 1;
            foreach (var account in accounts)
            {
                AddGroup(accounts.Count > 1 ? $"계정 {no}" : "계정",
                    F("아이디", account.id),
                    F("비밀번호", account.pw, sensitive: true, mono: true, maskAll: true));
                no++;
            }
            return "";
        }

        /// <summary>보안카드 코드표 열기 (데스크톱 F2 상세보기)</summary>
        [RelayCommand]
        private async Task OpenCodeTableAsync()
        {
            await Shell.Current.GoToAsync($"codes?key={ItemKey}");
        }

        [RelayCommand]
        private async Task EditAsync()
        {
            await Shell.Current.GoToAsync($"edit?category={(int)Meta.Id}&key={ItemKey}");
        }

        /// <summary>데스크톱 Del. 서버에서는 사용불가 처리된다.</summary>
        [RelayCommand]
        private async Task DeleteAsync()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page == null)
                return;

            bool confirm = await page.DisplayAlert(
                $"{Meta.Title} 삭제",
                $"'{Headline}' 을(를) 삭제할까요?\n서버에서는 사용불가 처리됩니다.",
                "삭제",
                "취소");

            if (confirm == false)
                return;

            IsBusy = true;
            try
            {
                var result = await _data.DeleteAsync(Meta.Id, ItemKey);
                if (result.Success == false)
                {
                    ErrorText = result.Message;
                    return;
                }

                await Shell.Current.GoToAsync("..");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
