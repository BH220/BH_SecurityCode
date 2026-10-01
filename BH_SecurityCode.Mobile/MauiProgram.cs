using BH_SecurityCode.Api.Interface;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Mobile.Services;
using BH_SecurityCode.Mobile.ViewModels;
using BH_SecurityCode.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace BH_SecurityCode.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            // 폰트는 지정하지 않는다. 기기 기본 한글 폰트를 그대로 써야 한글·영문이 어긋나지 않는다.
            // (Resources/Styles/Styles.xaml 주석 참고)

            RemoveAndroidInputUnderline();

            // ── 서버 API (BH_SecurityCode.Api) ─────────────────────
            // 엔드포인트·파라미터·응답 파싱은 모두 Api 프로젝트가 갖고 있다. 모바일은 호출만 한다.
            builder.Services.AddSingleton<IAuthManager, AuthManager>();
            builder.Services.AddSingleton<IBankCodeManager, BankCodeManager>();
            builder.Services.AddSingleton<IBankBookManager, BankBookManager>();
            builder.Services.AddSingleton<ICardManager, CardManager>();
            builder.Services.AddSingleton<IIdCardManager, IdCardManager>();
            builder.Services.AddSingleton<IAccountManager, AccountManager>();
            builder.Services.AddSingleton<ICodeManager, CodeManager>();
            builder.Services.AddSingleton<IImageManager, ImageManager>();

            // ── 앱 서비스 ──────────────────────────────────────────
            builder.Services.AddSingleton<IAppSettings, AppSettings>();
            builder.Services.AddSingleton<ICredentialStore, CredentialStore>();
            builder.Services.AddSingleton<IImageCache, ImageCache>();
            builder.Services.AddSingleton<IVaultDataService, ApiVaultDataService>();
            builder.Services.AddSingleton<ISessionService, SessionService>();

            // ── 화면 + ViewModel ───────────────────────────────────
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<LoginPage>();

            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<HomePage>();

            builder.Services.AddSingleton<SearchViewModel>();
            builder.Services.AddSingleton<SearchPage>();

            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<SettingsPage>();

            // 밀어서 여는 화면은 열 때마다 새로 만든다 (쿼리 파라미터가 매번 다르다).
            builder.Services.AddTransient<VaultListViewModel>();
            builder.Services.AddTransient<VaultListPage>();

            builder.Services.AddTransient<VaultDetailViewModel>();
            builder.Services.AddTransient<VaultDetailPage>();

            builder.Services.AddTransient<VaultEditViewModel>();
            builder.Services.AddTransient<VaultEditPage>();

            builder.Services.AddTransient<CodeTableViewModel>();
            builder.Services.AddTransient<CodeTablePage>();

            builder.Services.AddTransient<ServerSettingViewModel>();
            builder.Services.AddTransient<ServerSettingPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // 저장된 서버 주소를 ScApi 에 적용한다. (없으면 로그인 화면에서 설정하도록 안내한다)
            ApiBootstrap.Apply(app.Services.GetRequiredService<IAppSettings>());

            return app;
        }

        /// <summary>
        /// 안드로이드 기본 입력 컨트롤은 아래쪽에 밑줄을 그린다. 이 앱은 입력을 Border(InputShell)로
        /// 감싸므로 밑줄이 겹쳐 보인다. 플랫폼 뷰의 배경 틴트를 지워 밑줄만 없앤다.
        /// </summary>
        private static void RemoveAndroidInputUnderline()
        {
#if ANDROID
            var clear = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
                "BhNoUnderline", (handler, _) => handler.PlatformView.BackgroundTintList = clear);

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(
                "BhNoUnderline", (handler, _) => handler.PlatformView.BackgroundTintList = clear);

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(
                "BhNoUnderline", (handler, _) => handler.PlatformView.BackgroundTintList = clear);

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(
                "BhNoUnderline", (handler, _) => handler.PlatformView.BackgroundTintList = clear);

            Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping(
                "BhNoUnderline", (handler, _) => handler.PlatformView.BackgroundTintList = clear);
#endif
        }
    }
}
