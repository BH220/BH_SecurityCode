using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH_SecurityCode.Core.Common
{
    public class FilePathHelper
    {
        private static string GetPath(params string[] paths)
        {
            string path = Path.Combine(paths);
            // 확장자 있으면 파일로 간주 → 부모 폴더만 생성
            string? dir = Path.HasExtension(path)
                ? Path.GetDirectoryName(path)
                : path;

            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return path;
        }

        /// <summary>
        /// 기준이 되는 데이터 폴더. 윈도우는 %ProgramData%.
        /// 안드로이드/리눅스에서는 CommonApplicationData 가 "/usr/share"(쓰기 불가)로 잡히고 c:\ 도 쓸 수 없어서
        /// 앱 전용 저장소로 대체한다. (BH_SecurityCode.Mobile 이 Core 를 참조하므로 필요하다. 윈도우 동작은 그대로다.)
        /// </summary>
        private static string PlatformDataRoot()
        {
            if (OperatingSystem.IsWindows())
            {
                string common = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (string.IsNullOrEmpty(common) == false)
                    return common;
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        #region 기준 경로
        public static string ProgramDataRoot { get { return GetPath(PlatformDataRoot(), "BH Soft"); } }
        public static string Root { get { return OperatingSystem.IsWindows() ? GetPath(@"c:\Bh Soft") : GetPath(PlatformDataRoot(), "Bh Soft"); } }
        public static string AppRoot { get { return GetPath(Root, BhSecurityCodeDisplay); } }
        public static string SettingRoot { get { return GetPath(ProgramDataRoot, "settings"); } }
        public static string LogRoot { get { return GetPath(Root, "logs", BhSecurityCodeDisplay); } }
        //public static string AiRoot { get { return GetPath(Root, "ai_server"); } }
        //public static string DataRoot { get { return GetPath(ProgramDataRoot, "security"); } }
        //public static string RegistryRoot { get { return @"SOFTWARE\BH Soft"; } }
        //public static string RegistryAiRoot { get { return @"SOFTWARE\Deepnoid\AI Server"; } }
        //public static string RegistryAppRoot { get { return @"SOFTWARE\Deepnoid\Security"; } }
        //public static string RegistryUninstallRoot { get { return @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\"; } }
        //public static string ImageSaveRoot { get { return GetPath(@"D:\deepnoid\image\security"); } }
        //public static string ImageRoot { get { return GetPath(Root, "image_server"); } }
        //public static string LicenseDirectory { get { return GetPath(Root, "license"); } }
        //public static string LicensePath { get { return GetPath(LicenseDirectory, "LicenseFile.lfx"); } }
        //public static string UtilRoot { get { return GetPath(AppRoot, "utils"); } }

        //public static string StarterRoot { get { return GetPath(UtilRoot, "starter"); } }
        //public static string MonitoringRoot { get { return GetPath(UtilRoot, "monitor"); } }
        //public static string RoiSelectorRoot { get { return GetPath(UtilRoot, "roi_selector"); } }
        //public static string OnnxTesterRoot { get { return GetPath(UtilRoot, "onnx_tester"); } }
        //public static string PublishRoot { get { return GetPath(@"C:\deepnoid\publish"); } }
        #endregion

        #region 프로그램 표시명
        public static string BhSecurityCodeDisplay { get { return "BH Security Code"; } }
        //public static string ImageDisplay { get { return "Deep Security Image Store"; } }
        //public static string AiDisplay { get { return "Deep Security AI Server"; } }
        //public static string DatabaseDisplay { get { return "Maria Database"; } }
        //public static string UninstallDisplay { get { return "Uninstall"; } }
        //public static string InstallDisplay { get { return "Install"; } }
        //public static string CudaDisplay { get { return "Cuda"; } }
        //public static string CudnnDisplay { get { return "Cudnn"; } }
        //public static string VcRedistDisplay { get { return "VC Redist"; } }
        //public static string LogViwerDisplay { get { return "Log Viwer"; } }
        //public static string StarterDisplay { get { return "Deep Security Starter"; } }
        //public static string MonitorDisplay { get { return "Deep Security Monitor"; } }
        //public static string RoiSelectorDisplay { get { return "Deep Security Roi Selector"; } }
        //public static string OnnxTesterDisplay { get { return "Deep Security Onnx Tester"; } }
        #endregion

        #region 프로그램 실행 파일명
        //public static string SecurityExecuteName { get { return "DeepSecurity.exe"; } }
        //public static string ImageExecuteName { get { return "DeepSecurity.ImageStore.exe"; } }
        //public static string AiExecuteName { get { return "DeepSecurity.AI.exe"; } }
        //public static string UnInstallerTempExecuteName { get { return "DeepSecurity.Uninstaller.exe"; } }
        //public static string UnInstallerExecuteName { get { return "uninstaller.exe"; } }
        //public static string InstallerExecuteName { get { return "DeepSecurity.Installer.exe"; } }
        //public static string VcRedistExecuteName { get { return "vc_redist_x64.exe"; } }
        //public static string LogViewerExecuteName { get { return "LogViewer.exe"; } }
        //public static string StarterExecuteName { get { return "DeepSecurity.Starter.exe"; } }
        //public static string MonitoringExecuteName { get { return "DeepSecurity.Monitor.exe"; } }
        //public static string RoiSelectorExecuteName { get { return "DeepSecurity.Support.RoiSelector.exe"; } }
        //public static string DotNetSdkExecuteName { get { return "dotnet-sdk-8.0.410-win-x64.exe"; } }
        //public static string OnnxTesterExecuteName { get { return "DeepSecurity.Onnx.Tester.exe"; } }
        //public static string MariaDbFileName { get { return "mariadb-11.3.0-winx64.msi"; } }
        //public static string CudaFileName { get { return "cuda_12.8.1_572.61_windows.exe"; } }
        //public static string CudnnFileName { get { return "cudnn_9.11.0_cuda_12.8.zip"; } }
        //public static string CudnnOverwriteExecuteName { get { return "cuDnnOverwrite.bat"; } }
        #endregion
                
        #region Install
        public static string UpdateDirectory { get { return GetPath(AppRoot, "Updates"); } }
        //public static string DotNetSdkFullPath { get { return GetPath(SetupFileFolder, DotNetSdkExecuteName); } }
        //public static string UnInstallerExecuteFullPath { get { return GetPath(Root, UnInstallerExecuteName); } }

        //public static string PublishDeepSecurityDirectory { get { return GetPath(PublishRoot, "security"); } }
        //public static string PublishDeepSecurityUtilsDirectory { get { return GetPath(PublishDeepSecurityDirectory, "utils"); } }
        //public static string PublishDatabaseDirectory { get { return GetPath(PublishRoot, "database"); } }
        //public static string PublishImageStoreDirectory { get { return GetPath(PublishRoot, "image_store"); } }
        //public static string PublishAiServerDirectory { get { return GetPath(PublishRoot, "ai_server"); } }
        //public static string PublishAiServerAddonDirectory { get { return GetPath(PublishRoot, "ai_server_addon"); } }
        //public static string PublishInstallDirectory { get { return GetPath(PublishRoot, "install"); } }
        //public static string PublishUninstallDirectory { get { return GetPath(PublishRoot, "uninstall"); } }
        //public static string PublishStarterDirectory { get { return GetPath(PublishRoot, "security_starter"); } }
        //public static string PublishMonitorDirectory { get { return GetPath(PublishRoot, "security_monitor"); } }
        //public static string PublishRoiSelectorDirectory { get { return GetPath(PublishRoot, "roi_selector"); } }
        //public static string PublishOnnxTesterDirectory { get { return GetPath(PublishRoot, "onnx_tester"); } }

        //public static string PublishStarterDirectoryInSecurity { get { return GetPath(PublishDeepSecurityUtilsDirectory, "starter"); } }
        //public static string PublishMonitoringDirectoryInSecurity { get { return GetPath(PublishDeepSecurityUtilsDirectory, "monitor"); } }
        //public static string PublishRoiSelectorDirectoryInSecurity { get { return GetPath(PublishDeepSecurityUtilsDirectory, "roi_selector"); } }
        //public static string PublishOnnxTesterDirectoryInSecurity { get { return GetPath(PublishDeepSecurityUtilsDirectory, "onnx_tester"); } }


        //public static string PublishDeepSecurityFullPath { get { return GetPath(PublishDeepSecurityDirectory, SecurityExecuteName); } }
        //public static string PublishImageStoreFullPath { get { return GetPath(PublishImageStoreDirectory, ImageExecuteName); } }
        //public static string PublishAiServerFullPath { get { return GetPath(PublishAiServerDirectory, AiExecuteName); } }
        //public static string PublishDatabaseFullPath { get { return GetPath(PublishDatabaseDirectory, MariaDbFileName); } }
        //public static string PublishInstallFullPath { get { return GetPath(PublishInstallDirectory, InstallerExecuteName); } }
        //public static string PublishUninstallFullPath { get { return GetPath(PublishUninstallDirectory, UnInstallerTempExecuteName); } }
        //public static string PublishCudaFullPath { get { return GetPath(PublishAiServerAddonDirectory, CudaFileName); } }
        //public static string PublishCudnnFullPath { get { return GetPath(PublishAiServerAddonDirectory, CudnnFileName); } }
        //public static string PublishVcRedistFullPath { get { return GetPath(PublishAiServerAddonDirectory, VcRedistExecuteName); } }
        //public static string PublishLogViwerFullPath { get { return GetPath(PublishAiServerAddonDirectory, LogViewerExecuteName); } }
        //public static string PublishStarterFullPath { get { return GetPath(PublishStarterDirectory, StarterExecuteName); } }
        //public static string PublishMonitorFullPath { get { return GetPath(PublishMonitorDirectory, MonitoringExecuteName); } }
        //public static string PublishRoiSelectorFullPath { get { return GetPath(PublishRoiSelectorDirectory, RoiSelectorExecuteName); } }
        //public static string PublishOnnxTesterFullPath { get { return GetPath(PublishOnnxTesterDirectory, OnnxTesterExecuteName); } }

        //public static string AiPublishAirOnnxDll { get { return GetPath(PublishAiServerDirectory, "AddFiles", "air", "mmdeploy.dll"); } }
        //public static string AiPublishEntOnnxDll { get { return GetPath(PublishAiServerDirectory, "AddFiles", "ent", "mmdeploy.dll"); } }
        #endregion

    }
}
