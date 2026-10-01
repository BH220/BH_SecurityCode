using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

namespace BH_SecurityCode.Mobile
{
    /// <summary>
    /// 대상 기기(갤럭시 A35)는 세로 전용 단말로 취급한다.
    /// 키보드가 올라올 때 화면을 밀지 않고 리사이즈해 입력 필드가 가려지지 않게 한다.
    /// </summary>
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        ConfigurationChanges = ConfigChanges.ScreenSize
                             | ConfigChanges.Orientation
                             | ConfigChanges.UiMode
                             | ConfigChanges.ScreenLayout
                             | ConfigChanges.SmallestScreenSize
                             | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ApplySystemBars();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            ApplySystemBars();
        }

        /// <summary>
        /// 상태바·내비게이션바를 앱 배경색과 맞춘다. 기본값은 스플래시 테마의 보라색이라
        /// 페이지 배경(Resources/Styles/Colors.xaml 의 LtBg / DkBg)과 이어지지 않는다.
        /// </summary>
        private void ApplySystemBars()
        {
            if (Window is null)
                return;

            bool dark = (Resources?.Configuration?.UiMode & UiMode.NightMask) == UiMode.NightYes;

            var pageBg = Android.Graphics.Color.ParseColor(dark ? "#0F1116" : "#F3F4F8");
            var barBg = Android.Graphics.Color.ParseColor(dark ? "#191C23" : "#FFFFFF");

            // API 35 에서 이 두 메서드는 deprecated 지만, MAUI 9 가 edge-to-edge 강제를 opt-out 하고
            // 있으므로 여전히 동작한다. edge-to-edge 로 전환하면 인셋 기반으로 다시 작성해야 한다.
#pragma warning disable CA1422
            Window.SetStatusBarColor(pageBg);
            Window.SetNavigationBarColor(barBg);
#pragma warning restore CA1422

            var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
            if (controller is not null)
            {
                controller.AppearanceLightStatusBars = dark == false;
                controller.AppearanceLightNavigationBars = dark == false;
            }
        }
    }
}
