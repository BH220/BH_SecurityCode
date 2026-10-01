using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Constants;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BH_SecurityCode.ViewModels.BankCode
{
    /// <summary>보안카드 목록 (기존 frmBankCodeList). 항목은 API 응답 <see cref="BankCodeInfo"/> 를 그대로 쓴다.</summary>
    public partial class BankCodeListViewModel : ListViewModelBase<BankCodeInfo>
    {
        private readonly IBankCodeManager _manager;
        private readonly IServiceProvider _services;

        public override Menus MenuId => Menus.보안코드;

        public BankCodeListViewModel(IBankCodeManager manager, IDialogService dialog, IServiceProvider services)
            : base(dialog)
        {
            _manager = manager;
            _services = services;
            Title = "보안카드 정보";
        }

        protected override void ConfigureButtons(FunctionButtonState buttons)
        {
            buttons.CopyEnabled = false;
            buttons.CodeSimpleViewVisible = true;
            buttons.CodeViewVisible = true;
        }

        /// <summary>목록 API 는 사용불가 건도 내려주므로 사용중인 것만 표시한다.</summary>
        protected override async Task<List<BankCodeInfo>> LoadItemsAsync()
        {
            var list = await _manager.GetListAsync();
            return list.Where(x => x.status == CdStatus.사용).ToList();
        }

        protected override string GetSearchText(BankCodeInfo item)
            => $"{item.owner_type} {item.name} {item.bank_type} {item.code_qty} {item.code_align} {item.note} {item.serial}";

        protected override Task InsertAsync() => OpenEditAsync(null);

        protected override Task UpdateAsync(BankCodeInfo item) => OpenEditAsync(item.bank_code_num);

        protected override async Task DeleteAsync(BankCodeInfo item)
        {
            var (success, message) = await _manager.DeleteAsync(item.bank_code_num);
            if (success)
                await RefreshAsync();
            else
                Dialog.ShowError(message);
        }

        protected override async Task CustomFunctionAsync(string functionId)
        {
            switch (functionId)
            {
                case Functions.코드간편보기: await ShowSimpleViewAsync(); break;
                case Functions.코드상세보기: await ShowDetailViewAsync(); break;
            }
        }

        private async Task OpenEditAsync(int? bankCodeNum)
        {
            var vm = _services.GetRequiredService<BankCodeEditViewModel>();
            if (await vm.LoadAsync(bankCodeNum) == false)
                return;
            if (Dialog.ShowDialog(vm) == true)
                await RefreshAsync();
        }

        /// <summary>간편보기 (F1, 행 더블클릭)</summary>
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async Task ShowSimpleViewAsync()
        {
            if (SelectedItem == null)
            {
                Dialog.ShowWarning("선택된 자료가 없습니다.");
                return;
            }
            NotifyActivity();
            var vm = _services.GetRequiredService<BankCodeSimpleViewViewModel>();
            if (await vm.LoadAsync(SelectedItem.bank_code_num))
                Dialog.ShowDialog(vm);
        }

        /// <summary>상세보기 (F2, 우클릭 메뉴)</summary>
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private async Task ShowDetailViewAsync()
        {
            if (SelectedItem == null)
            {
                Dialog.ShowWarning("선택된 자료가 없습니다.");
                return;
            }
            NotifyActivity();
            var vm = _services.GetRequiredService<BankCodeViewViewModel>();
            if (await vm.LoadAsync(SelectedItem.bank_code_num))
                Dialog.ShowDialog(vm);
        }
    }
}
