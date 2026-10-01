using System.Collections.ObjectModel;
using BH_SecurityCode.Core;
using BH_SecurityCode.Mobile.Models;
using BH_SecurityCode.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>홈 타일 하나. 데스크톱 좌측 메뉴 버튼에 대응한다.</summary>
    public sealed partial class HomeTileViewModel : ObservableObject
    {
        public HomeTileViewModel(CategoryMeta meta)
        {
            Meta = meta;
        }

        public CategoryMeta Meta { get; }

        [ObservableProperty]
        private int _count;

        public string CountText => Count > 0 ? $"{Count}건" : "-";

        partial void OnCountChanged(int value) => OnPropertyChanged(nameof(CountText));
    }

    /// <summary>
    /// 홈. 데스크톱 MainWindow(메뉴 + 상태바)를 모바일 대시보드로 바꾼 화면.
    /// </summary>
    public sealed partial class HomeViewModel : BaseViewModel
    {
        private readonly IVaultDataService _data;
        private readonly IAppSettings _settings;
        private readonly ICredentialStore _credentials;

        public HomeViewModel(IVaultDataService data, ISessionService session, IAppSettings settings,
            ICredentialStore credentials)
        {
            _data = data;
            _settings = settings;
            _credentials = credentials;
            Session = session;

            // 보안코드는 이 앱의 주 기능이라 넓은 카드로 따로 두고, 나머지 넷은 2x2 타일로 놓는다.
            HeroTile = new HomeTileViewModel(Categories.Code);
            Tiles.Add(HeroTile);
            foreach (var meta in Categories.All.Skip(1))
            {
                var tile = new HomeTileViewModel(meta);
                Tiles.Add(tile);
                OtherTiles.Add(tile);
            }
        }

        public ISessionService Session { get; }

        public HomeTileViewModel HeroTile { get; }

        /// <summary>건수 갱신용 전체 목록 (화면에는 HeroTile 과 OtherTiles 로 나눠서 그린다)</summary>
        public List<HomeTileViewModel> Tiles { get; } = new();

        public ObservableCollection<HomeTileViewModel> OtherTiles { get; } = new();

        public ObservableCollection<VaultRowViewModel> Recent { get; } = new();

        [ObservableProperty]
        private string _greeting = "";

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _errorText = "";

        public bool HasError => ErrorText.Length > 0;
        public bool HasRecent => Recent.Count > 0;

        public string ServerText =>
            string.IsNullOrEmpty(_settings.ServerAddress) ? "서버 미설정" : _settings.ServerAddress;

        partial void OnErrorTextChanged(string value) => OnPropertyChanged(nameof(HasError));

        [RelayCommand]
        private Task LoadAsync() => LoadCoreAsync(false);

        private async Task LoadCoreAsync(bool reload)
        {
            Greeting = DateTime.Now.Hour switch
            {
                < 6 => "늦은 시간이네요",
                < 12 => "좋은 아침입니다",
                < 18 => "안녕하세요",
                _ => "수고하셨습니다",
            };
            OnPropertyChanged(nameof(ServerText));

            if (IsBusy)
                return;

            IsBusy = true;
            ErrorText = "";
            try
            {
                var counts = await _data.GetCountsAsync(reload);
                foreach (var tile in Tiles)
                    tile.Count = counts.Data != null && counts.Data.TryGetValue(tile.Meta.Id, out int n) ? n : 0;

                TotalCount = counts.Data?.Values.Sum() ?? 0;

                if (counts.Success == false)
                {
                    ErrorText = counts.Message;
                    Recent.Clear();
                    OnPropertyChanged(nameof(HasRecent));
                    return;
                }

                // 최근 항목: 카테고리별 첫 건을 모아 보여준다.
                Recent.Clear();
                foreach (var meta in Categories.All)
                {
                    var items = await _data.GetItemsAsync(meta.Id);
                    var first = items.Data?.FirstOrDefault();
                    if (first != null)
                        Recent.Add(new VaultRowViewModel(first, _settings.MaskSensitive, ShowToast));
                }
                OnPropertyChanged(nameof(HasRecent));
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
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

        [RelayCommand]
        private async Task OpenCategoryAsync(HomeTileViewModel? tile)
        {
            if (tile == null)
                return;

            await Shell.Current.GoToAsync($"list?category={(int)tile.Meta.Id}");
        }

        [RelayCommand]
        private async Task OpenItemAsync(VaultRowViewModel? row)
        {
            if (row == null)
                return;

            await Shell.Current.GoToAsync($"detail?category={(int)row.Meta.Id}&key={row.Item.Key}");
        }

        /// <summary>데스크톱 Alt+R (시간 연장)</summary>
        [RelayCommand]
        private void Extend()
        {
            Session.Extend();
            ShowToast("세션 시간을 연장했습니다");
        }

        /// <summary>데스크톱 Alt+O (로그아웃). 자동 로그인과 저장된 비밀번호도 함께 해제한다.</summary>
        [RelayCommand]
        private async Task SignOutAsync()
        {
            _settings.AutoLogin = false;
            await _credentials.ClearAsync();
            await Session.SignOutAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
