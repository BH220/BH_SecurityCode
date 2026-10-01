using BH_SecurityCode.Mobile.Views;

namespace BH_SecurityCode.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // 밀어서 여는 화면들. 데스크톱의 모달 창 자리다.
            Routing.RegisterRoute("list", typeof(VaultListPage));
            Routing.RegisterRoute("detail", typeof(VaultDetailPage));
            Routing.RegisterRoute("codes", typeof(CodeTablePage));
            Routing.RegisterRoute("edit", typeof(VaultEditPage));
            Routing.RegisterRoute("serversetting", typeof(ServerSettingPage));
        }
    }
}
