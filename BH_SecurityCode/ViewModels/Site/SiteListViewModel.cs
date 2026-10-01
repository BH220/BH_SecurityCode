using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BH_SecurityCode.ViewModels.Site
{
    /// <summary>계정(사이트) 목록 (기존 frmAccountList). 항목은 API 응답 <see cref="AccountInfo"/> 를 그대로 쓴다. (id = 대표 아이디)</summary>
    public partial class SiteListViewModel : ListViewModelBase<AccountInfo>
    {
        private readonly IAccountManager _manager;
        private readonly IServiceProvider _services;

        public override Menus MenuId => Menus.계정;

        public SiteListViewModel(IAccountManager manager, IDialogService dialog, IServiceProvider services)
            : base(dialog)
        {
            _manager = manager;
            _services = services;
            Title = "계정 정보";
        }

        protected override Task<List<AccountInfo>> LoadItemsAsync() => _manager.GetListAsync();

        protected override string GetSearchText(AccountInfo item)
            => $"{item.owner} {item.name} {item.id} {item.address} {item.note}";

        protected override Task InsertAsync() => OpenEditAsync(null);

        protected override Task UpdateAsync(AccountInfo item) => OpenEditAsync(item.account_num);

        protected override async Task DeleteAsync(AccountInfo item)
        {
            var (success, message) = await _manager.DeleteAsync(item.account_num);
            if (success)
                await RefreshAsync();
            else
                Dialog.ShowError(message);
        }

        private async Task OpenEditAsync(int? accountNum)
        {
            var vm = _services.GetRequiredService<SiteEditViewModel>();
            if (await vm.LoadAsync(accountNum) == false)
                return;
            if (Dialog.ShowDialog(vm) == true)
                await RefreshAsync();
        }
    }
}
