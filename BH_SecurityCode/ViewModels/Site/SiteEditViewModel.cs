using System.Collections.ObjectModel;
using System.Diagnostics;
using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.ViewModels.Site
{
    /// <summary>
    /// 계정(사이트) 추가/수정 (기존 frmAccountInfo + ctlAccountGrid).
    /// API 응답 <see cref="AccountInfo"/> 기준으로 동작하며 하위 계정은 <see cref="AccountItemInfo"/> 목록(items)으로 다룬다.
    /// </summary>
    public partial class SiteEditViewModel : EditViewModelBase
    {
        private readonly IAccountManager _manager;

        /// <summary>수정 대상 account_num. 0 이면 신규</summary>
        private int _accountNum;

        public ObservableCollection<AccountItemInfo> Accounts { get; } = new();

        /// <summary>삭제된 기존 하위 계정의 account_detail_num 목록 (매니저가 status 사용불가 처리)</summary>
        public List<int> DeletedAccountIds { get; } = new();

        [ObservableProperty] private string _name = "";
        [ObservableProperty] private string _address = "";
        [ObservableProperty] private string _note = "";
        [ObservableProperty] private bool _isNameFocused;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ViewAccountCommand), nameof(DeleteAccountCommand))]
        private AccountItemInfo? _selectedAccount;

        public SiteEditViewModel(IAccountManager manager, ICodeManager codes, IDialogService dialog)
            : base(dialog, codes)
        {
            _manager = manager;
        }

        /// <summary>accountNum 이 null/0 이면 추가, 아니면 상세 API 로 불러와 수정 모드로 연다.</summary>
        public async Task<bool> LoadAsync(int? accountNum)
        {
            await LoadOwnersAsync();

            if (accountNum == null || accountNum <= 0)
            {
                IsNew = true;
                Title = "계정정보 추가";
                _accountNum = 0;
                Accounts.Clear();
                DeletedAccountIds.Clear();
            }
            else
            {
                var info = await _manager.GetAsync(accountNum.Value);
                if (info == null)
                {
                    Dialog.ShowError("계정정보를 불러오는데 실패 하였습니다.");
                    return false;
                }
                IsNew = false;
                Title = "계정정보 수정";
                _accountNum = info.account_num;

                Name = info.name ?? "";
                Address = info.address ?? "";
                Note = info.note ?? "";
                SelectOwner(info.owner);

                Accounts.Clear();
                DeletedAccountIds.Clear();
                foreach (var item in info.items)
                    Accounts.Add(item);
                SelectedAccount = Accounts.FirstOrDefault();
            }

            IsNameFocused = true;
            return true;
        }

        protected override bool Validate(out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(Name))
            {
                error = "사이트명을 입력하세요.";
                IsNameFocused = true;
                return false;
            }
            if (string.IsNullOrWhiteSpace(Address))
            {
                error = "주소를 입력하세요.";
                return false;
            }
            return true;
        }

        protected override async Task<(bool Success, string Message)> SaveAsync()
        {
            var info = new AccountInfo
            {
                account_num = IsNew ? 0 : _accountNum,
                owner = SelectedOwnerType,
                name = Name.Trim(),
                address = Address.Trim(),
                note = Note.Trim(),
                status = CdStatus.사용,
                items = Accounts.ToList(), // 신규(account_detail_num 0)/변경 하위 계정. 매니저가 sub/update 로 처리
            };

            var result = IsNew
                ? await _manager.InsertAsync(info)
                : await _manager.UpdateAsync(info, DeletedAccountIds);

            if (result.Success)
            {
                IsNew = false;
                DeletedAccountIds.Clear();
            }
            return result;
        }

        protected override void ResetForContinue()
        {
            _accountNum = 0;
            Name = "";
            Address = "";
            Note = "";
            Accounts.Clear();
            DeletedAccountIds.Clear();
            Title = "계정정보 추가";
            IsNameFocused = true;
        }

        #region 계정 목록
        private bool HasSelectedAccount() => SelectedAccount != null;

        [RelayCommand]
        private void AddAccount()
        {
            var vm = new AccountDetailViewModel(null);
            if (Dialog.ShowDialog(vm) != true)
                return;
            var item = vm.ToModel();
            Accounts.Add(item);
            SelectedAccount = item;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedAccount))]
        private void ViewAccount()
        {
            if (SelectedAccount == null)
            {
                Dialog.ShowWarning("선택된 계정 정보가 없습니다.");
                return;
            }
            var vm = new AccountDetailViewModel(SelectedAccount);
            if (Dialog.ShowDialog(vm) != true)
                return;
            int idx = Accounts.IndexOf(SelectedAccount);
            var updated = vm.ToModel();
            Accounts[idx] = updated;
            SelectedAccount = updated;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedAccount))]
        private void DeleteAccount()
        {
            if (SelectedAccount == null)
                return;
            if (Dialog.Confirm("계정을 삭제하시겠습니까?", "계정 삭제") == false)
                return;
            if (SelectedAccount.account_detail_num > 0)
                DeletedAccountIds.Add(SelectedAccount.account_detail_num);
            Accounts.Remove(SelectedAccount);
            SelectedAccount = Accounts.FirstOrDefault();
        }

        /// <summary>주소로 바로가기 (기존 btnRunSite)</summary>
        [RelayCommand]
        private void OpenSite()
        {
            string url = Address.Trim();
            if (string.IsNullOrEmpty(url))
            {
                Dialog.ShowWarning("주소가 입력되지 않았습니다.");
                return;
            }
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == false &&
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == false)
                url = "https://" + url;

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "OpenSite failed");
                Dialog.ShowError($"사이트를 열 수 없습니다.\r\n{ex.Message}");
            }
        }
        #endregion
    }
}
