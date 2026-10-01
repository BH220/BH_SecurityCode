using BH_SecurityCode.Api;
using BH_SecurityCode.Api.Manager;
using BH_SecurityCode.Common.Configurations;
using BH_SecurityCode.Core.Common;
using BH_SecurityCode.Core.Helper;
using BH_SecurityCode.Core.Utils;
using BH_SecurityCode.Services;
using BH_SecurityCode.ViewModels;
using BH_SecurityCode.ViewModels.BankBook;
using BH_SecurityCode.ViewModels.BankCode;
using BH_SecurityCode.ViewModels.Card;
using BH_SecurityCode.ViewModels.IdCard;
using BH_SecurityCode.ViewModels.Site;
using BH_SecurityCode.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using BH_SecurityCode.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace BH_SecurityCode
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern Boolean AllocConsole();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MINIMIZE = 6;

        public IServiceProvider? Services { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        uint lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        uint hTemplateFile);

        private const int MY_CODE_PAGE = 949;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_WRITE = 0x2;
        private const uint OPEN_EXISTING = 0x3;

        private Mutex? _mutex;
        private const string MutexName = "BH_SecurityCode_Wpf_SingleInstance";
        public static string LoginPw = "";
        public static bool IsLockScreen = false;
        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, MutexName, out bool createdNew);
            if (createdNew == false)
            {
                MessageBox.Show("BH Security Code 가 이미 실행 중입니다.", "BH Security Code", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }


            bool normalExecution = false;
            string[] args = e.Args; // 실행 인수
#if DEBUG
            normalExecution = true;
#else 
            if (args != null || args.Length == 1)
                normalExecution = true;
#endif
            if (normalExecution)
            {
                // 알림/작업 표시줄용 앱 ID. 창을 만들기 전에 등록해야 한다.
                RegisterAppUserModelId();

                //관리자 권한 변경
                SelfElevatedProcess();
                //시작전 메모리에 남아 있는 프로세스가 있으면 죽이고 시작
                ProcessCleaner();

                ShowConsoleWindow(); 

                // 1. 기본 WPF 어플리케이션 초기화
                base.OnStartup(e);

                // DI 컨테이너 구성
                Services = DiService.ServicesRegister();

                // 로깅 설정, 기타 초기화
                Log.Configure("BH Security Code");

                // 2. 어플리케이션 종료 모드 설정
                ShutdownMode = ShutdownMode.OnExplicitShutdown;

                MakeShortCut();

                Log.Info("[System] Program Start");

                DispatcherUnhandledException += OnDispatcherUnhandledException;

                var appSettings = Services.GetRequiredService<IAppSettings>();
                appSettings.Load();

                // MainWindow 설정 및 수동 Show
                var view = Services.GetRequiredService<MainWindow>();
                ShowWindow(view);
            }
            else
                AppExit();
        }

        private static void ShowWindow(Window window)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //window.Left = 
            //window.Top = 
            //window.Width = 
            //window.Height = 
            window.Closed += Window_Closed;
            window.WindowState = WindowState.Normal;
            window.Show();
        }

        private static void Window_Closed(object? sender, EventArgs e)
        {
            if (Current != null)
            {
                foreach (Window win in Current.Windows)
                {
                    if (win != sender as Window)
                        win.Close();
                }
            }
            Application.Current.Shutdown();
            Environment.Exit(0); // 혹시 모를 잔여 프로세스를 강제로 종료
        }
        private async void AppExit()
        {
            //var dialog = new InvalidLaunchMessage($"{FilePathHelper.LogViewerAppRoot}{FilePathHelper.LauncherExecute}", 530);
            //dialog.ShowDialog();
            Environment.Exit(0);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Flush();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // API 연동 전 기능(ApiNotImplementedException)은 오류가 아닌 "구현 필요" 안내로 보여준다.
            var notImplemented = FindNotImplemented(e.Exception);
            if (notImplemented != null)
            {
                Log.Warn(notImplemented.Message.Replace("\r\n", " "));
                var dialog = Services?.GetService<IDialogService>();
                if (dialog != null)
                    dialog.ShowInfo(notImplemented.Message, "구현 필요");
                else
                    MessageBox.Show(notImplemented.Message, "구현 필요", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Handled = true;
                return;
            }

            Log.Exception(e.Exception, "Unhandled exception");
            MessageBox.Show($"처리되지 않은 오류가 발생했습니다.\r\n{e.Exception.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static ApiNotImplementedException? FindNotImplemented(Exception? ex)
        {
            while (ex != null)
            {
                if (ex is ApiNotImplementedException api)
                    return api;
                if (ex is AggregateException agg && agg.InnerExceptions.Count > 0)
                    ex = agg.InnerExceptions[0];
                else
                    ex = ex.InnerException;
            }
            return null;
        }


        private void SelfElevatedProcess()
        {
            Log.Info("관리자 권한으로 실행준비...");

            if (!IsRunAsAdmin())
            {
                Log.Info("관리자 권한이 없으므로 관리자 권한으로 실행");

                ProcessStartInfo proc = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Environment.ProcessPath,
                    Verb = "runas"
                };

                Log.Info("관리자권한으로 실행 -- 관리자권한으로 실행할 프로세스 : " + proc.FileName);

                try
                {
                    Process.Start(proc);
                    Log.Info("관리자 권한으로 실행...");

                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    Log.Info("관리자 실행 실패: " + ex.Message);
                }
            }
        }
        private static bool IsRunAsAdmin()
        {
            bool isAdmin = false;
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
            }
            return isAdmin;
        }

        private void ShowConsoleWindow()
        {
            bool OpenConsole = false;
#if DEBUG
            OpenConsole = true;
#else
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "CTest.dat"))
                OpenConsole = true;
#endif
            if (OpenConsole)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                if (!AllocConsole())
                    MessageBox.Show("Console Window Load Failed");
                else
                {
                    IntPtr stdHandle = CreateFile("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0);
                    SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
                    FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                    Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
                    StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
                    standardOutput.AutoFlush = true;
                    Console.SetOut(standardOutput);
                    Log.Info("This will show up in the Console window.");
                }
            }
        }

        private void ProcessCleaner()
        {
            try
            {
                string currentName = Process.GetCurrentProcess().ProcessName;
                var currentId = Process.GetCurrentProcess().Id;
                var processes = Process.GetProcessesByName(currentName);

                foreach (var p in processes)
                {
                    if (p.Id != currentId)
                    {
                        Log.Info($"ProcessCleaner - Found another process with the same name: {p.ProcessName} (PID: {p.Id}). Attempting to kill it.");
                        p.Kill(); // 다른 동일 이름 프로세스 종료
                    }
                }
            }
            catch { }
        }


        /// <summary>
        /// 알림(트레이 풍선/토스트)·작업 표시줄에서 이 앱을 식별하는 ID.
        /// 시작 메뉴 바로가기의 System.AppUserModel.ID 와 같아야 알림 제목이 "BH Security Code" 로 나온다.
        /// </summary>
        public const string AppUserModelId = "BHSoft.BHSecurityCode";

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

        /// <summary>창을 만들기 전에 호출해야 한다.</summary>
        private static void RegisterAppUserModelId()
        {
            try
            {
                int hr = SetCurrentProcessExplicitAppUserModelID(AppUserModelId);
                if (hr != 0)
                    Log.Warn($"SetCurrentProcessExplicitAppUserModelID 실패: 0x{hr:X8}");
            }
            catch (Exception ex)
            {
                Log.Warn($"AppUserModelID 등록 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 바탕 화면 + 시작 메뉴에 바로가기를 만든다. (관리자 권한 실행 플래그, AppUserModel.ID 포함)
        /// 시작 메뉴 바로가기는 Windows 알림이 앱 이름/아이콘을 찾는 근거이므로 반드시 만든다.
        /// </summary>
        private void MakeShortCut()
        {
            string appPath = Environment.ProcessPath;
            string shortcutName = "BH Security Code.lnk"; // 생성할 바로가기 파일 이름
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

            try
            {
                foreach (string dir in new[] { desktopPath, startMenuPath })
                {
                    // COM 타입 라이브러리 없이 IShellLinkW 로 바로가기 생성
                    ShellLink.CreateShortcut(
                        Path.Combine(dir, shortcutName),
                        appPath,
                        Path.GetDirectoryName(appPath),
                        "BH Security Code",
                        appPath,
                        runAsAdministrator: true,
                        appUserModelId: AppUserModelId);
                }
                RegistryHelper.AddKey(Microsoft.Win32.RegistryHive.CurrentUser, "", "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers", appPath, "~ RUNASADMIN");
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "바로가기 생성 실패");
            }
        }
    }
}
