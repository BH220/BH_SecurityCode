using System.Collections.ObjectModel;
using BH_SecurityCode.Core;
using BH_SecurityCode.Mobile.Models;
using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>
    /// 카테고리 하나의 목록. 데스크톱 ListViewModelBase&lt;T&gt; + DataGrid 를 모바일 카드 목록으로 옮긴 것.
    /// 상단 기능 버튼(Ins/F8/Del/F5/F6) 대신 검색창 · 소유자 필터 · 아래로 당겨 새로고침을 쓴다.
    /// </summary>
    public sealed partial class VaultListViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IVaultDataService _data;
        private readonly IAppSettings _settings;
        private List<VaultItem> _all = new();

        public VaultListViewModel(IVaultDataService data, IAppSettings settings)
        {
            _data = data;
            _settings = settings;
        }

        public ObservableCollection<VaultRowViewModel> Items { get; } = new();

        /// <summary>소유자 필터 칩. 첫 항목은 "전체".</summary>
        public ObservableCollection<ChipViewModel> Owners { get; } = new();

        [ObservableProperty]
        private int _categoryId;

        [ObservableProperty]
        private CategoryMeta _meta = Categories.Code;

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private string _selectedOwner = "전체";

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _emptyText = "";

        /// <summary>서버 오류 메시지. 비어있지 않으면 목록 대신 오류 상태를 보여준다.</summary>
        [ObservableProperty]
        private string _errorText = "";

        public bool HasError => ErrorText.Length > 0;
        public bool IsEmpty => IsBusy == false && HasError == false && Items.Count == 0;

        /// <summary>Shell 이 넘겨준 list?category=101 을 받는다.</summary>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("category", out object? value) &&
                int.TryParse(value?.ToString(), out int id))
                CategoryId = id;
        }

        partial void OnCategoryIdChanged(int value)
        {
            Meta = Categories.Get((Menus)value);
            Title = Meta.Title;
            _ = LoadCoreAsync(false);
        }

        partial void OnErrorTextChanged(string value)
        {
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(IsEmpty));
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();

        partial void OnSelectedOwnerChanged(string value) => ApplyFilter();

        [RelayCommand]
        private Task LoadAsync() => LoadCoreAsync(false);

        private async Task LoadCoreAsync(bool reload)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorText = "";
            OnPropertyChanged(nameof(IsEmpty));
            try
            {
                var result = await _data.GetItemsAsync(Meta.Id, reload);
                if (result.Success == false)
                {
                    _all = new List<VaultItem>();
                    Items.Clear();
                    Owners.Clear();
                    ErrorText = result.Message;
                    return;
                }

                _all = result.Data ?? new List<VaultItem>();

                // 소유자 칩은 이름(가나다)이 아니라 소유자 코드(CdOwner 값) 순으로 놓는다. "전체" 는 항상 맨 앞.
                Owners.Clear();
                Owners.Add(new ChipViewModel("전체", SelectedOwner == "전체"));
                foreach (string owner in _all
                    .GroupBy(x => x.OwnerName)
                    .OrderBy(g => g.Min(x => x.OwnerCode))
                    .Select(g => g.Key))
                    Owners.Add(new ChipViewModel(owner, SelectedOwner == owner));

                ApplyFilter();
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        [RelayCommand]
        private Task RefreshAsync()
        {
            IsRefreshing = true;
            return LoadCoreAsync(true);
        }

        [RelayCommand]
        private Task RetryAsync() => LoadCoreAsync(true);

        private void ApplyFilter()
        {
            string keyword = (SearchText ?? "").Trim();
            IEnumerable<VaultItem> query = _all;

            if (string.IsNullOrEmpty(SelectedOwner) == false && SelectedOwner != "전체")
                query = query.Where(x => x.OwnerName == SelectedOwner);

            if (keyword.Length > 0)
                query = query.Where(x => x.SearchText.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            // 목록도 칩과 같은 기준(소유자 코드)으로 묶여 보이게 정렬한다.
            Items.Clear();
            foreach (var item in query.OrderBy(x => x.OwnerCode).ThenBy(x => x.Title))
                Items.Add(new VaultRowViewModel(item, _settings.MaskSensitive, ShowToast));

            EmptyText = keyword.Length > 0 || SelectedOwner != "전체"
                ? "조건에 맞는 항목이 없습니다"
                : $"{Meta.Title} 항목이 없습니다";

            OnPropertyChanged(nameof(IsEmpty));
        }

        [RelayCommand]
        private void SelectOwner(ChipViewModel? chip)
        {
            if (chip == null)
                return;

            foreach (var owner in Owners)
                owner.IsSelected = ReferenceEquals(owner, chip);

            SelectedOwner = chip.Text;
        }

        [RelayCommand]
        private async Task OpenAsync(VaultRowViewModel? row)
        {
            if (row == null)
                return;

            await Shell.Current.GoToAsync($"detail?category={(int)row.Meta.Id}&key={row.Item.Key}");
        }

        /// <summary>데스크톱 Ins(추가)</summary>
        [RelayCommand]
        private async Task AddAsync()
        {
            await Shell.Current.GoToAsync($"edit?category={(int)Meta.Id}&key=0");
        }
    }
}
