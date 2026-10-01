using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Constants;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BH_SecurityCode.ViewModels.BankBook
{
    /// <summary>통장 목록 (기존 frmBankBookList)</summary>
    public partial class BankBookListViewModel : ListViewModelBase<BankBookInfo>
    {
        private readonly IBankBookManager _manager;
        private readonly IServiceProvider _services;

        public override Menus MenuId => Menus.통장;

        public BankBookListViewModel(IBankBookManager manager, IDialogService dialog, IServiceProvider services)
            : base(dialog)
        {
            _manager = manager;
            _services = services;
            Title = "통장 정보";
        }

        protected override Task<List<BankBookInfo>> LoadItemsAsync() => _manager.GetListAsync();

        protected override string GetSearchText(BankBookInfo item)
            => $"{item.owner_type} {item.bank_book_num} {item.name} {item.owner_type} {item.book_no} {item.note}";

        protected override Task InsertAsync() => OpenEditAsync(null);

        protected override Task UpdateAsync(BankBookInfo item) => OpenEditAsync(item.bank_book_num);

        protected override async Task DeleteAsync(BankBookInfo item)
        {
            var (success, message) = await _manager.DeleteAsync(item.bank_book_num);
            if (success)
                await RefreshAsync();
            else
                Dialog.ShowError(message);
        }

        private async Task OpenEditAsync(int? sqBankBook)
        {
            var vm = _services.GetRequiredService<BankBookEditViewModel>();
            if (await vm.LoadAsync(sqBankBook) == false)
                return;
            if (Dialog.ShowDialog(vm) == true)
                await RefreshAsync();
        }
    }
}
