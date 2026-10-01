using System.Windows.Controls;

namespace BH_SecurityCode.Views.BankCode
{
    /// <summary>
    /// ViewTemplates.xaml 의 DataTemplate 이 생성하므로 매개변수 없는 생성자가 필요하다.
    /// DataContext(BankCodeListViewModel) 는 ContentControl 이 넘겨준다.
    /// </summary>
    public partial class BankCodeListView : UserControl
    {
        public BankCodeListView()
        {
            InitializeComponent();
        }
    }
}
