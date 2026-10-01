using System.Windows;
using BH_SecurityCode.ViewModels.BankBook;
using BH_SecurityCode.ViewModels.BankCode;
using BH_SecurityCode.ViewModels.Card;
using BH_SecurityCode.ViewModels.Common;
using BH_SecurityCode.ViewModels.IdCard;
using BH_SecurityCode.ViewModels.Site;
using BH_SecurityCode.Views.BankBook;
using BH_SecurityCode.Views.BankCode;
using BH_SecurityCode.Views.Card;
using BH_SecurityCode.Views.Common;
using BH_SecurityCode.Views.IdCard;
using BH_SecurityCode.Views.Site;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace BH_SecurityCode.Services
{
    public class DialogService : IDialogService
    {
        /// <summary>ViewModel → Window 매핑</summary>
        private static readonly Dictionary<Type, Type> WindowMap = new()
        {
            [typeof(MessageViewModel)] = typeof(MessageWindow),
            [typeof(ImageViewerViewModel)] = typeof(ImageViewerWindow),
            [typeof(ServerSettingViewModel)] = typeof(ServerSettingWindow),
            [typeof(BankCodeEditViewModel)] = typeof(BankCodeEditWindow),
            [typeof(BankCodeViewViewModel)] = typeof(BankCodeViewWindow),
            [typeof(BankCodeSimpleViewViewModel)] = typeof(BankCodeSimpleViewWindow),
            [typeof(BankBookEditViewModel)] = typeof(BankBookEditWindow),
            [typeof(CardEditViewModel)] = typeof(CardEditWindow),
            [typeof(IdCardEditViewModel)] = typeof(IdCardEditWindow),
            [typeof(IdTypeSelectorViewModel)] = typeof(IdTypeSelectorWindow),
            [typeof(SiteEditViewModel)] = typeof(SiteEditWindow),
            [typeof(AccountDetailViewModel)] = typeof(AccountDetailWindow),
        };

        public void ShowInfo(string message, string title = "알림")
            => ShowMessage(message, title, MessageKind.Info, MessageButtons.Ok);

        public void ShowWarning(string message, string title = "경고")
            => ShowMessage(message, title, MessageKind.Warning, MessageButtons.Ok);

        public void ShowError(string message, string title = "오류")
            => ShowMessage(message, title, MessageKind.Error, MessageButtons.Ok);

        public bool Confirm(string message, string title = "확인")
            => ShowMessage(message, title, MessageKind.Question, MessageButtons.YesNo) == MessageResult.Yes;

        public bool? ConfirmWithCancel(string message, string title = "확인")
        {
            return ShowMessage(message, title, MessageKind.Question, MessageButtons.YesNoCancel) switch
            {
                MessageResult.Yes => true,
                MessageResult.No => false,
                _ => null,
            };
        }

        public bool? ShowDialog(ObservableObject viewModel)
        {
            if (WindowMap.TryGetValue(viewModel.GetType(), out var windowType) == false)
                throw new InvalidOperationException($"{viewModel.GetType().Name} 에 매핑된 Window 가 없습니다.");

            var window = (Window)Activator.CreateInstance(windowType)!;
            window.DataContext = viewModel;
            window.Owner = GetActiveWindow();
            window.WindowStartupLocation = window.Owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            return window.ShowDialog();
        }

        public string? OpenFile(string filter, string title = "파일 선택")
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title,
                Multiselect = false,
            };
            return dialog.ShowDialog(GetActiveWindow()) == true ? dialog.FileName : null;
        }

        private MessageResult ShowMessage(string message, string title, MessageKind kind, MessageButtons buttons)
        {
            var vm = new MessageViewModel(title, message, kind, buttons);
            ShowDialog(vm);
            return vm.Result;
        }

        private static Window? GetActiveWindow()
        {
            var windows = Application.Current.Windows.OfType<Window>().ToList();
            return windows.FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;
        }
    }

    public class ClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
            try
            {
                Clipboard.SetText(text ?? "");
            }
            catch
            {
                // 다른 프로세스가 클립보드를 점유 중인 경우 무시
            }
        }
    }
}
