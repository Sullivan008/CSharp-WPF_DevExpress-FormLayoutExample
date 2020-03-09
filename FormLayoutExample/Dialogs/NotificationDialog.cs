using DevExpress.Xpf.Core;
using System.Windows;

namespace FormLayoutExample.Dialogs
{
    public class NotificationDialog
    {
        private readonly string _title;
        private readonly string _content;

        public NotificationDialog(string title, string content)
        {
            _title = title;
            _content = content;
        }

        public MessageBoxResult ShowMessageBoxByMessageType(MessageBoxImage messageImage) =>
            DXMessageBox.Show(_content, _title, MessageBoxButton.OK, messageImage);
    }
}
