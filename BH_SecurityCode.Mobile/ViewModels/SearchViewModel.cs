using System.Collections.ObjectModel;
using BH_SecurityCode.Mobile.Models;
using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>검색 결과 묶음 (카테고리별).</summary>
    public sealed class SearchGroup : List<VaultRowViewModel>
    {
        public SearchGroup(CategoryMeta meta, IEnumerable<VaultRowViewModel> rows) : base(rows)
        {
            Meta = meta;
        }

        public CategoryMeta Meta { get; }
        public string Title => Meta.Title;
        public string CountText => $"{Count}건";
    }

    /// <summary>
    /// 통합 검색. 데스크톱은 화면마다 F6 검색이 따로였지만, 모바일에서는 다섯 종류를 한 번에 찾는다.
    /// </summary>
    public sealed partial class SearchViewModel : BaseViewModel
    {
        private readonly IVaultDataService _data;
        private readonly IAppSettings _settings;
        private readonly List<VaultItem> _all = new();

        public SearchViewModel(IVaultDataService data, IAppSettings settings)
        {
            _data = data;
            _settings = settings;
        }

        public ObservableCollection<SearchGroup> Groups { get; } = new();

        [ObservableProperty]
        private string _keyword = "";

        [ObservableProperty]
        private int _resultCount;

        [ObservableProperty]
        private bool _hasSearched;

        [ObservableProperty]
        private string _errorText = "";

        public bool HasError => ErrorText.Length > 0;
        public bool ShowEmpty => HasSearched && ResultCount == 0 && IsBusy == false && HasError == false;
        public bool ShowHint => HasSearched == false && IsBusy == false && HasError == false;

        partial void OnErrorTextChanged(string value)
        {
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(ShowEmpty));
            OnPropertyChanged(nameof(ShowHint));
        }

        partial void OnKeywordChanged(string value) => _ = SearchAsync();

        [RelayCommand]
        private Task LoadAsync() => LoadCoreAsync(false);

        private async Task LoadCoreAsync(bool reload)
        {
            IsBusy = true;
            ErrorText = "";
            try
            {
                _all.Clear();
                foreach (var meta in Categories.All)
                {
                    var result = await _data.GetItemsAsync(meta.Id, reload);
                    if (result.Success == false)
                    {
                        ErrorText = result.Message;
                        return;
                    }
                    _all.AddRange(result.Data ?? new List<VaultItem>());
                }

            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(ShowHint));
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            await LoadCoreAsync(false);

            string keyword = (Keyword ?? "").Trim();
            Groups.Clear();

            if (keyword.Length == 0)
            {
                HasSearched = false;
                ResultCount = 0;
                OnPropertyChanged(nameof(ShowEmpty));
                OnPropertyChanged(nameof(ShowHint));
                return;
            }

            var hits = _all
                .Where(x => x.SearchText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var meta in Categories.All)
            {
                var rows = hits
                    .Where(x => x.Category == meta.Id)
                    .OrderBy(x => x.Title)
                    .Select(x => new VaultRowViewModel(x, _settings.MaskSensitive, ShowToast))
                    .ToList();

                if (rows.Count > 0)
                    Groups.Add(new SearchGroup(meta, rows));
            }

            ResultCount = hits.Count;
            HasSearched = true;
            OnPropertyChanged(nameof(ShowEmpty));
            OnPropertyChanged(nameof(ShowHint));
        }

        [RelayCommand]
        private Task RetryAsync() => LoadCoreAsync(true);

        [RelayCommand]
        private async Task OpenAsync(VaultRowViewModel? row)
        {
            if (row == null)
                return;

            await Shell.Current.GoToAsync($"detail?category={(int)row.Meta.Id}&key={row.Item.Key}");
        }
    }
}
