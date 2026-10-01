using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BH_SecurityCode.ViewModels.IdCard
{
    /// <summary>신분증 목록 (기존 frmIdCardList). 항목은 API 응답 <see cref="IdCardInfo"/> 를 그대로 쓴다.</summary>
    public partial class IdCardListViewModel : ListViewModelBase<IdCardInfo>
    {
        private readonly IIdCardManager _manager;
        private readonly IServiceProvider _services;

        public override Menus MenuId => Menus.신분증;

        public IdCardListViewModel(IIdCardManager manager, IDialogService dialog, IServiceProvider services)
            : base(dialog)
        {
            _manager = manager;
            _services = services;
            Title = "신분증 정보";
        }

        protected override Task<List<IdCardInfo>> LoadItemsAsync() => _manager.GetListAsync();

        protected override string GetSearchText(IdCardInfo item)
            => $"{item.owner} {item.id_type} {item.name} {item.name_kor} {item.license_org} {item.note}";

        protected override async Task InsertAsync()
        {
            // 추가 시 신분증 종류를 먼저 선택한다. (기존 frmIdSelector)
            var selector = new IdTypeSelectorViewModel();
            if (Dialog.ShowDialog(selector) != true)
                return;
            await OpenEditAsync(null, selector.SelectedType);
        }

        protected override Task UpdateAsync(IdCardInfo item) => OpenEditAsync(item.id_num, item.id_type);

        protected override async Task DeleteAsync(IdCardInfo item)
        {
            var (success, message) = await _manager.DeleteAsync(item.id_num);
            if (success)
                await RefreshAsync();
            else
                Dialog.ShowError(message);
        }

        private async Task OpenEditAsync(int? idNum, CdIdType idType)
        {
            var vm = _services.GetRequiredService<IdCardEditViewModel>();
            if (await vm.LoadAsync(idNum, idType) == false)
                return;
            if (Dialog.ShowDialog(vm) == true)
                await RefreshAsync();
        }
    }
}
