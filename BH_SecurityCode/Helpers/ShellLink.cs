using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace BH_SecurityCode.Helpers
{
    /// <summary>
    /// Windows 바로가기(.lnk) 생성. 타입 라이브러리(COMReference) 없이 IShellLinkW 를 직접 선언해 사용하므로
    /// dotnet CLI 빌드에서도 동작한다.
    /// </summary>
    public static class ShellLink
    {
        /// <summary>IShellLinkDataList 플래그: 관리자 권한으로 실행</summary>
        private const uint SLDF_RUNAS_USER = 0x00002000;

        /// <summary>
        /// 바로가기를 만든다. 이미 있으면 덮어쓴다.
        /// </summary>
        /// <param name="shortcutPath">생성할 .lnk 전체 경로</param>
        /// <param name="targetPath">실행 파일 경로</param>
        /// <param name="workingDirectory">작업 폴더</param>
        /// <param name="description">설명(툴팁)</param>
        /// <param name="iconPath">아이콘 파일 경로 (null 이면 targetPath)</param>
        /// <param name="runAsAdministrator">true 면 "관리자 권한으로 실행" 옵션을 켠다</param>
        /// <param name="appUserModelId">
        /// 알림/작업 표시줄에서 앱을 식별하는 AppUserModel.ID. 앱이 SetCurrentProcessExplicitAppUserModelID 로 선언한 값과 같아야 하며,
        /// 시작 메뉴 바로가기에 심어 두면 트레이 풍선(토스트) 제목이 "Microsoft.Explorer.Notification.{GUID}" 대신 프로그램 이름으로 나온다.
        /// </param>
        public static void CreateShortcut(string shortcutPath, string targetPath, string? workingDirectory = null,
            string? description = null, string? iconPath = null, bool runAsAdministrator = false, string? appUserModelId = null)
        {
            var link = (IShellLinkW)new CShellLink();
            try
            {
                link.SetPath(targetPath);
                link.SetWorkingDirectory(workingDirectory ?? Path.GetDirectoryName(targetPath) ?? "");
                link.SetDescription(description ?? "");
                link.SetIconLocation(iconPath ?? targetPath, 0);

                if (runAsAdministrator && link is IShellLinkDataList dataList)
                {
                    dataList.GetFlags(out uint flags);
                    dataList.SetFlags(flags | SLDF_RUNAS_USER);
                }

                if (string.IsNullOrEmpty(appUserModelId) == false)
                    ShellLinkProperties.SetAppUserModelId(link, appUserModelId);

                ((IPersistFile)link).Save(shortcutPath, true);
            }
            finally
            {
                Marshal.ReleaseComObject(link);
            }
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class CShellLink { }

        [ComImport]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out ushort pwHotkey);
            void SetHotkey(ushort wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [Guid("45E2B4AE-B1C3-11D0-B92F-00A0C90312E1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkDataList
        {
            void AddDataBlock(IntPtr pDataBlock);
            void CopyDataBlock(uint dwSig, out IntPtr ppDataBlock);
            void RemoveDataBlock(uint dwSig);
            void GetFlags(out uint pdwFlags);
            void SetFlags(uint dwFlags);
        }
    }
}
