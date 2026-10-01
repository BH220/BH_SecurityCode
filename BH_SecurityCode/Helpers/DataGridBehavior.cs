using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BH_SecurityCode.Helpers
{
    /// <summary>
    /// DataGrid 첨부 동작.
    /// - RowDoubleClickCommand : 행 더블클릭 시 명령 실행 (헤더/빈 영역 더블클릭은 무시)
    /// - SelectRowOnRightClick : 우클릭한 행을 먼저 선택 (컨텍스트 메뉴의 수정/삭제가 우클릭한 행을 대상으로 하도록)
    /// </summary>
    public static class DataGridBehavior
    {
        #region RowDoubleClickCommand
        public static readonly DependencyProperty RowDoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("RowDoubleClickCommand", typeof(ICommand), typeof(DataGridBehavior),
                new PropertyMetadata(null, OnRowDoubleClickCommandChanged));

        public static ICommand? GetRowDoubleClickCommand(DependencyObject obj) => (ICommand?)obj.GetValue(RowDoubleClickCommandProperty);
        public static void SetRowDoubleClickCommand(DependencyObject obj, ICommand? value) => obj.SetValue(RowDoubleClickCommandProperty, value);

        private static void OnRowDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid)
                return;

            grid.MouseDoubleClick -= OnMouseDoubleClick;
            if (e.NewValue != null)
                grid.MouseDoubleClick += OnMouseDoubleClick;
        }

        private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid || e.OriginalSource is not DependencyObject source)
                return;

            if (FindRow(grid, source) is not DataGridRow row)
                return;

            var command = GetRowDoubleClickCommand(grid);
            if (command?.CanExecute(row.Item) == true)
            {
                command.Execute(row.Item);
                e.Handled = true;
            }
        }
        #endregion

        #region SelectRowOnRightClick
        public static readonly DependencyProperty SelectRowOnRightClickProperty =
            DependencyProperty.RegisterAttached("SelectRowOnRightClick", typeof(bool), typeof(DataGridBehavior),
                new PropertyMetadata(false, OnSelectRowOnRightClickChanged));

        public static bool GetSelectRowOnRightClick(DependencyObject obj) => (bool)obj.GetValue(SelectRowOnRightClickProperty);
        public static void SetSelectRowOnRightClick(DependencyObject obj, bool value) => obj.SetValue(SelectRowOnRightClickProperty, value);

        private static void OnSelectRowOnRightClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid)
                return;

            grid.PreviewMouseRightButtonDown -= OnPreviewMouseRightButtonDown;
            if ((bool)e.NewValue)
                grid.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
        }

        private static void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid || e.OriginalSource is not DependencyObject source)
                return;

            if (FindRow(grid, source) is not DataGridRow row)
                return;

            // 우클릭한 행을 선택 상태로 만든다. (컨텍스트 메뉴는 SelectedItem 을 대상으로 동작)
            if (row.IsSelected == false)
            {
                grid.SelectedItem = row.Item;
                row.IsSelected = true;
            }
            row.Focus();
        }
        #endregion

        private static DataGridRow? FindRow(DataGrid grid, DependencyObject source)
            => ItemsControl.ContainerFromElement(grid, source) as DataGridRow;
    }
}
