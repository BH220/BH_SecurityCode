using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.ViewModels.Base
{
    /// <summary>
    /// 상단 기능 버튼(추가/수정/삭제/...)의 활성화·표시 상태.
    /// 각 목록 화면(IFunctionHost)이 소유하고 MainViewModel 이 바인딩한다.
    /// </summary>
    public partial class FunctionButtonState : ObservableObject
    {
        [ObservableProperty] private bool _insertEnabled;
        [ObservableProperty] private bool _updateEnabled;
        [ObservableProperty] private bool _deleteEnabled;
        [ObservableProperty] private bool _copyEnabled;
        [ObservableProperty] private bool _refreshEnabled;
        [ObservableProperty] private bool _searchEnabled;
        [ObservableProperty] private bool _excelEnabled;
        [ObservableProperty] private bool _printEnabled;
        [ObservableProperty] private bool _closeEnabled;

        /// <summary>보안카드 간편보기 (F1) 버튼 표시 여부</summary>
        [ObservableProperty] private bool _codeSimpleViewVisible;

        /// <summary>보안카드 상세보기 (F2) 버튼 표시 여부</summary>
        [ObservableProperty] private bool _codeViewVisible;

        public void SetAll(bool enabled)
        {
            InsertEnabled = UpdateEnabled = DeleteEnabled = CopyEnabled = RefreshEnabled =
                SearchEnabled = ExcelEnabled = PrintEnabled = CloseEnabled = enabled;
        }
    }
}
