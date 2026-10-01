using BH_SecurityCode.Mobile.Services;

namespace BH_SecurityCode.Mobile
{
    public partial class App : Application
    {
        private readonly ISessionService _session;

        public App(ISessionService session)
        {
            InitializeComponent();
            _session = session;

            // 세션이 만료되면 어느 화면에 있든 로그인으로 돌린다. (데스크톱 자동 잠금과 같은 동작)
            _session.Expired += OnSessionExpired;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            Dispatcher.Dispatch(async () =>
            {
                if (Shell.Current != null)
                    await Shell.Current.GoToAsync("//login");
            });
        }
    }
}
