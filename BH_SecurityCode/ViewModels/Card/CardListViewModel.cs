using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Api.Model.Response;
using BH_SecurityCode.Core;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BH_SecurityCode.ViewModels.Card
{
    /// <summary>카드 목록 (기존 frmCardList). 항목은 API 응답 <see cref="CardInfo"/> 를 그대로 쓴다.</summary>
    public partial class CardListViewModel : ListViewModelBase<CardInfo>
    {
        private readonly ICardManager _manager;
        private readonly IServiceProvider _services;

        public override Menus MenuId => Menus.카드;

        public CardListViewModel(ICardManager manager, IDialogService dialog, IServiceProvider services)
            : base(dialog)
        {
            _manager = manager;
            _services = services;
            Title = "카드 정보";
        }

        protected override Task<List<CardInfo>> LoadItemsAsync() => _manager.GetListAsync();

        protected override string GetSearchText(CardInfo item)
            => $"{item.owner} {item.card_type} {item.name} {item.owner_name} {item.card_no} {item.expire_date} {item.note}";

        protected override Task InsertAsync() => OpenEditAsync(null);

        protected override Task UpdateAsync(CardInfo item) => OpenEditAsync(item.card_num);

        protected override async Task DeleteAsync(CardInfo item)
        {
            var (success, message) = await _manager.DeleteAsync(item.card_num);
            if (success)
                await RefreshAsync();
            else
                Dialog.ShowError(message);
        }

        private async Task OpenEditAsync(int? cardNum)
        {
            var vm = _services.GetRequiredService<CardEditViewModel>();
            if (await vm.LoadAsync(cardNum) == false)
                return;
            if (Dialog.ShowDialog(vm) == true)
                await RefreshAsync();
        }
    }
}
