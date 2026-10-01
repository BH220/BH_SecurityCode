using System.Drawing;
using System.Windows;
using BH_SecurityCode.Core.Helper;
using WinForms = System.Windows.Forms;

namespace BH_SecurityCode.Services
{
    /// <summary>
    /// 시스템 트레이 아이콘. 창의 X 는 종료가 아니라 트레이로 숨기기이고, 실제 종료는
    /// 상단의 [프로그램 종료] 버튼이나 트레이 메뉴의 [프로그램 종료] 로만 한다.
    /// (WPF 에는 NotifyIcon 이 없어 WinForms 의 것을 쓴다. csproj 의 UseWindowsForms 참고)
    /// </summary>
    public sealed class TrayIconService : IDisposable
    {
        private const string Title = "BH Security Code";

        private readonly Window _window;
        private readonly Action _exit;
        private readonly WinForms.NotifyIcon _icon;
        private bool _tipShown;

        /// <param name="window">숨기고 되살릴 메인 창</param>
        /// <param name="exit">트레이 메뉴 [프로그램 종료] 동작 (확인 → 로그아웃 → 종료를 하는 명령)</param>
        public TrayIconService(Window window, Action exit)
        {
            _window = window;
            _exit = exit;

            var menu = new WinForms.ContextMenuStrip();
            menu.Items.Add("열기(&O)", null, (_, _) => Restore());
            menu.Items.Add(new WinForms.ToolStripSeparator());
            menu.Items.Add("프로그램 종료(&X)", null, (_, _) => { Restore(); _exit(); });

            _icon = new WinForms.NotifyIcon
            {
                Text = Title,
                Icon = LoadIcon(),
                ContextMenuStrip = menu,
                Visible = true,
            };
            _icon.DoubleClick += (_, _) => Restore();
        }

        /// <summary>창을 숨기고 트레이에만 남긴다. 처음 한 번은 안내 풍선을 띄운다.</summary>
        public void HideToTray()
        {
            _window.Hide();
            if (_tipShown)
                return;
            _tipShown = true;
            _icon.ShowBalloonTip(2500, Title, "트레이에서 계속 실행 중입니다. 종료는 트레이 메뉴나 [프로그램 종료] 버튼을 사용하세요.", WinForms.ToolTipIcon.Info);
        }

        /// <summary>트레이에서 창을 다시 보인다.</summary>
        public void Restore()
        {
            _window.Show();
            if (_window.WindowState == WindowState.Minimized)
                _window.WindowState = WindowState.Normal;
            _window.Activate();
        }

        private static Icon LoadIcon()
        {
            try
            {
                var resource = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/app.ico"));
                if (resource != null)
                    return new Icon(resource.Stream);
            }
            catch (Exception ex)
            {
                Log.Warn($"트레이 아이콘 로드 실패: {ex.Message}");
            }
            return SystemIcons.Application;
        }

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
        }
    }
}
