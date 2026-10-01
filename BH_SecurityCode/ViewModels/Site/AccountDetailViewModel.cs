using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.ViewModels.Site
{
    /// <summary>
    /// 계정 아이디/비밀번호 입력 (기존 frmAccountDetailInfo).
    /// 항목은 API 의 하위 계정 모델 <see cref="AccountItemInfo"/> (account_detail_num / id / pw / status) 를 쓴다.
    /// </summary>
    public partial class AccountDetailViewModel : DialogViewModelBase
    {
        private readonly AccountItemInfo _source;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        private string _userId = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private bool _isIdFocused;

        [ObservableProperty]
        private string _errorMessage = "";

        public bool IsNew { get; }

        public AccountDetailViewModel(AccountItemInfo? item)
        {
            IsNew = item == null;
            _source = item ?? new AccountItemInfo();
            Title = IsNew ? "계정 추가" : "계정 정보";
            UserId = _source.id ?? "";
            Password = _source.pw ?? "";
            IsIdFocused = true;
        }

        /// <summary>입력값이 반영된 하위 계정 모델을 반환한다. (account_detail_num 0 이면 신규)</summary>
        public AccountItemInfo ToModel() => new()
        {
            account_detail_num = _source.account_detail_num,
            id = UserId.Trim(),
            pw = Password,
            status = CdStatus.사용,
        };

        private bool CanOk() => string.IsNullOrWhiteSpace(UserId) == false;

        [RelayCommand(CanExecute = nameof(CanOk))]
        private void Ok()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                ErrorMessage = "아이디를 입력하세요.";
                IsIdFocused = true;
                return;
            }
            DialogResult = true;
        }
    }
}
