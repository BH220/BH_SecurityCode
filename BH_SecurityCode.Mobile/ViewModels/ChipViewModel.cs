using CommunityToolkit.Mvvm.ComponentModel;

namespace BH_SecurityCode.Mobile.ViewModels
{
    /// <summary>
    /// 필터 칩 한 개 (소유자 · 표시 방식).
    ///
    /// CollectionView 의 SelectionMode=Single 을 쓰지 않는다. 안드로이드에서는 선택된 항목 컨테이너에
    /// 시스템 테마의 강조색 배경이 깔려 칩 모양 뒤로 사각형이 비친다. 선택 상태를 직접 들고
    /// DataTrigger 로만 칠한다.
    /// </summary>
    public sealed partial class ChipViewModel : ObservableObject
    {
        public ChipViewModel(string text, bool isSelected = false)
        {
            Text = text;
            _isSelected = isSelected;
        }

        public string Text { get; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
