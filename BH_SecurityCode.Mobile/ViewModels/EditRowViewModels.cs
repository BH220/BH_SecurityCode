using BH_SecurityCode.Api.Model.Response;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>보안카드 코드 한 칸의 입력. 순번은 고정, 값만 입력한다.</summary>
    public sealed partial class CodeEntryViewModel : ObservableObject
    {
        public CodeEntryViewModel(int no, string code)
        {
            No = no;
            _code = code ?? "";
        }

        public int No { get; }
        public string NoText => No.ToString("00");

        [ObservableProperty]
        private string _code = "";
    }

    /// <summary>계정 하위 아이디/비밀번호 한 줄.</summary>
    public sealed partial class AccountEntryViewModel : ObservableObject
    {
        private readonly Action<AccountEntryViewModel> _remove;

        public AccountEntryViewModel(AccountItemInfo? source, Action<AccountEntryViewModel> remove)
        {
            _remove = remove;
            DetailNum = source?.account_detail_num ?? 0;
            _id = source?.id ?? "";
            _password = source?.pw ?? "";
        }

        /// <summary>기존 항목의 account_detail_num. 새로 추가한 줄은 0.</summary>
        public int DetailNum { get; }

        [ObservableProperty]
        private string _id = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private bool _isPasswordHidden = true;

        public bool IsEmpty => string.IsNullOrWhiteSpace(Id) && string.IsNullOrWhiteSpace(Password);

        [RelayCommand]
        private void ToggleReveal() => IsPasswordHidden = !IsPasswordHidden;

        [RelayCommand]
        private void Remove() => _remove(this);
    }

    /// <summary>편집 화면의 첨부 한 장. 기존 이미지와 새로 고른 이미지를 함께 다룬다.</summary>
    public sealed partial class EditAttachmentViewModel : ObservableObject
    {
        private readonly Action<EditAttachmentViewModel> _remove;

        public EditAttachmentViewModel(ImageInfo image, Action<EditAttachmentViewModel> remove, ImageSource? preview = null)
        {
            Image = image;
            _remove = remove;
            _preview = preview;
        }

        public ImageInfo Image { get; }

        /// <summary>아직 서버에 없는 첨부인지</summary>
        public bool IsNew => Image.IsNew;

        public string FileName => Image.FileName;
        public string SizeText => Image.SizeText;
        public string StateText => IsNew ? "새 첨부" : "등록됨";

        [ObservableProperty]
        private ImageSource? _preview;

        public bool HasPreview => Preview != null;

        partial void OnPreviewChanged(ImageSource? value) => OnPropertyChanged(nameof(HasPreview));

        [RelayCommand]
        private void Remove() => _remove(this);
    }
}
