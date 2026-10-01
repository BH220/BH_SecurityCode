using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.Services
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title = "알림");
        void ShowWarning(string message, string title = "경고");
        void ShowError(string message, string title = "오류");

        /// <summary>예/아니오 확인. 예=true</summary>
        bool Confirm(string message, string title = "확인");

        /// <summary>예/아니오/취소 확인. 예=true, 아니오=false, 취소=null</summary>
        bool? ConfirmWithCancel(string message, string title = "확인");

        /// <summary>ViewModel 에 매핑된 Window 를 모달로 연다.</summary>
        bool? ShowDialog(ObservableObject viewModel);

        /// <summary>파일 선택 대화상자. 취소 시 null</summary>
        string? OpenFile(string filter, string title = "파일 선택");
    }

    public interface IClipboardService
    {
        void SetText(string text);
    }
}
