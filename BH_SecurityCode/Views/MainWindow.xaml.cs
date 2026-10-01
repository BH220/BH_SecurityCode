using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;
using Wpf.Ui;

namespace BH_SecurityCode.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _provider;
        private TrayIconService? _tray;

        public MainWindow(IServiceProvider provider, MainViewModel viewModel)
        {
            InitializeComponent();
            _provider = provider;
            DataContext = viewModel;
            this.Closing += MainWindow_Closing;
            this.Closed += (_, _) => _tray?.Dispose();
            this.Loaded += MainView_Loaded;
        }

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            var snackbar = _provider.GetRequiredService<ISnackbarService>();
            snackbar.SetSnackbarPresenter(SnackbarPresenter);

            if (DataContext is not MainViewModel vm) return;

            // 트레이 메뉴의 [프로그램 종료] 는 상단 버튼과 같은 명령(확인 → 로그아웃 → 종료)을 탄다.
            _tray ??= new TrayIconService(this, () => vm.ExitCommand.Execute(null));
            vm.InitializeAsync();
        }

        /// <summary>
        /// 창 닫기(X) 는 종료가 아니라 트레이로 숨기기다.
        /// 실제 종료는 [프로그램 종료] 버튼 / 트레이 메뉴 → MainViewModel.ExitAsync 가 확인·로그아웃을 마치고
        /// AllowClose 를 켠 뒤 Application.Shutdown 으로 들어오며, 그때만 창이 진짜 닫힌다.
        /// </summary>
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
                return;
            if (vm.AllowClose)
                return;

            e.Cancel = true;
            _tray?.HideToTray();
        }
    }
}
