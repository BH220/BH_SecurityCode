using System.Runtime.InteropServices;

namespace BH_SecurityCode.Helpers
{
    /// <summary>
    /// 바로가기(.lnk)의 셸 속성 저장소(IPropertyStore) 접근.
    /// System.AppUserModel.ID 를 바로가기에 심어 두면 Windows 가 이 앱의 알림(트레이 풍선/토스트)·작업 표시줄 그룹에
    /// "Microsoft.Explorer.Notification.{GUID}" 대신 바로가기 이름과 아이콘을 쓴다.
    /// (앱 쪽에서는 SetCurrentProcessExplicitAppUserModelID 로 같은 ID 를 선언해야 하며, 시작 메뉴에 바로가기가 있어야 매핑된다)
    /// </summary>
    internal static class ShellLinkProperties
    {
        /// <summary>PKEY_AppUserModel_ID = {9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}, 5</summary>
        private static readonly Guid AppUserModelFormatId = new("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");
        private const uint AppUserModelIdPropertyId = 5;
        private const ushort VT_LPWSTR = 31;

        /// <summary>IShellLink COM 개체에 AppUserModel.ID 를 설정한다. Save 전에 호출.</summary>
        public static void SetAppUserModelId(object shellLink, string appUserModelId)
        {
            if (shellLink is not IPropertyStore store)
                throw new InvalidOperationException("IShellLink 개체가 IPropertyStore 를 지원하지 않습니다.");

            var key = new PropertyKey { fmtid = AppUserModelFormatId, pid = AppUserModelIdPropertyId };
            var value = new PropVariant { vt = VT_LPWSTR, pointerValue = Marshal.StringToCoTaskMemUni(appUserModelId) };
            try
            {
                Marshal.ThrowExceptionForHR(store.SetValue(ref key, ref value));
                Marshal.ThrowExceptionForHR(store.Commit());
            }
            finally
            {
                Marshal.FreeCoTaskMem(value.pointerValue);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct PropertyKey
        {
            public Guid fmtid;
            public uint pid;
        }

        /// <summary>PROPVARIANT (VT_LPWSTR 만 사용). x86/x64 모두 vt 뒤 6바이트 예약 후 union 이 온다.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct PropVariant
        {
            public ushort vt;
            public ushort reserved1;
            public ushort reserved2;
            public ushort reserved3;
            public IntPtr pointerValue;
            public IntPtr pointerValue2;
        }

        [ComImport]
        [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            [PreserveSig] int GetCount(out uint count);
            [PreserveSig] int GetAt(uint index, out PropertyKey key);
            [PreserveSig] int GetValue(ref PropertyKey key, out PropVariant value);
            [PreserveSig] int SetValue(ref PropertyKey key, ref PropVariant value);
            [PreserveSig] int Commit();
        }
    }
}
