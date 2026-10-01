using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using BH_SecurityCode.Api;
using BH_SecurityCode.Core;
using BH_SecurityCode.Core.Constants;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Messages;
using BH_SecurityCode.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BH_SecurityCode.ViewModels.Base
{
    /// <summary>
    /// 목록 화면 공통 ViewModel. (기존 frmXxxList : BhForm 의 공통 동작)
    /// </summary>
    public abstract partial class ListViewModelBase<T> : ViewModelBase, IFunctionHost where T : class
    {
        protected readonly IDialogService Dialog;
        protected readonly IMessenger Messenger;

        public abstract Menus MenuId { get; }
        public FunctionButtonState Buttons { get; } = new();

        public ObservableCollection<T> Items { get; } = new();
        public ICollectionView ItemsView { get; }

        [ObservableProperty]
        private T? _selectedItem;

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private bool _isSearchFocused;

        [ObservableProperty]
        private int _totalCount;

        protected ListViewModelBase(IDialogService dialog)
        {
            Dialog = dialog;
            Messenger = WeakReferenceMessenger.Default;

            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = o => o is T item && MatchesSearch(item, SearchText);

            Buttons.SetAll(true);
            Buttons.CopyEnabled = false;
            ConfigureButtons(Buttons);
        }

        partial void OnSearchTextChanged(string value) => ItemsView.Refresh();

        /// <summary>화면별 기능 버튼 상태 조정</summary>
        protected virtual void ConfigureButtons(FunctionButtonState buttons) { }

        public virtual async Task InitializeAsync() => await RefreshAsync();

        protected abstract Task<List<T>> LoadItemsAsync();

        /// <summary>검색 대상 문자열 (공백으로 연결)</summary>
        protected abstract string GetSearchText(T item);

        protected abstract Task InsertAsync();
        protected abstract Task UpdateAsync(T item);
        protected abstract Task DeleteAsync(T item);
        protected virtual Task CopyAsync(T item) => Task.CompletedTask;
        protected virtual Task CustomFunctionAsync(string functionId) => Task.CompletedTask;

        protected virtual bool MatchesSearch(T item, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;
            return GetSearchText(item).Contains(text.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            NotifyActivity();
            int preIndex = SelectedItem != null ? Items.IndexOf(SelectedItem) : -1;
            IsBusy = true;
            try
            {
                var list = await LoadItemsAsync();
                Items.Clear();
                foreach (var item in list)
                    Items.Add(item);
                TotalCount = Items.Count;

                if (Items.Count > 0)
                {
                    if (preIndex < 0) preIndex = 0;
                    if (preIndex > Items.Count - 1) preIndex = Items.Count - 1;
                    SelectedItem = Items[preIndex];
                }
                SetStatus($"{Title} - {Items.Count}건");
            }
            catch (ApiNotImplementedException ex)
            {
                // API 연동 전: 오류가 아닌 "구현 필요" 안내. 화면은 빈 목록으로 열린다.
                Items.Clear();
                TotalCount = 0;
                SetStatus($"{Title} - API 연동 전");
                Dialog.ShowInfo(ex.Message, "구현 필요");
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"{GetType().Name} Refresh failed");
                Dialog.ShowError($"자료를 불러오는 중 오류가 발생했습니다.\r\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Insert()
        {
            NotifyActivity();
            await InsertAsync();
        }

        [RelayCommand]
        private async Task Update()
        {
            if (SelectedItem == null)
            {
                Dialog.ShowWarning("수정할 자료가 선택되지 않았습니다.");
                return;
            }
            NotifyActivity();
            await UpdateAsync(SelectedItem);
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (SelectedItem == null)
            {
                Dialog.ShowWarning("삭제할 자료가 선택되지 않았습니다.");
                return;
            }
            if (Dialog.Confirm("선택한 자료를 삭제하시겠습니까?", "삭제 확인") == false)
                return;
            NotifyActivity();
            await DeleteAsync(SelectedItem);
        }

        [RelayCommand]
        private void Search()
        {
            IsSearchFocused = false;
            IsSearchFocused = true;
        }

        public async Task RunFunctionAsync(string functionId)
        {
            switch (functionId)
            {
                case Functions.추가: if (Buttons.InsertEnabled) await InsertCommand.ExecuteAsync(null); break;
                case Functions.수정: if (Buttons.UpdateEnabled) await UpdateCommand.ExecuteAsync(null); break;
                case Functions.삭제: if (Buttons.DeleteEnabled) await DeleteCommand.ExecuteAsync(null); break;
                case Functions.갱신: if (Buttons.RefreshEnabled) await RefreshCommand.ExecuteAsync(null); break;
                case Functions.검색: if (Buttons.SearchEnabled) SearchCommand.Execute(null); break;
                case Functions.복사:
                    if (Buttons.CopyEnabled && SelectedItem != null) await CopyAsync(SelectedItem);
                    break;
                case Functions.엑셀:
                    // TODO: 엑셀 내보내기 구현 (기존 BhForm.ExportExcel)
                    if (Buttons.ExcelEnabled) Dialog.ShowInfo("엑셀 내보내기 기능은 현재 준비중 입니다.");
                    break;
                case Functions.인쇄:
                    // TODO: 인쇄 구현 (기존 BhForm.ExportPrint)
                    if (Buttons.PrintEnabled) Dialog.ShowInfo("인쇄 기능은 현재 준비중 입니다.");
                    break;
                default:
                    await CustomFunctionAsync(functionId);
                    break;
            }
        }

        protected void NotifyActivity() => Messenger.Send(new UserActivityMessage());

        protected void SetStatus(string text) => Messenger.Send(new StatusTextMessage(text));
    }
}
