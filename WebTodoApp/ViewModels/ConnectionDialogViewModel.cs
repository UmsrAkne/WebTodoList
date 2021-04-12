using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTodoApp.Models;

namespace WebTodoApp.ViewModels {
    class ConnectionDialogViewModel : IDialogAware {
        public string Title => "サーバーの情報を入力";

        public event Action<IDialogResult> RequestClose;

        public AnyDBConnectionStrings DBConnectionStrings = new AnyDBConnectionStrings();

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() {
        }

        public void OnDialogOpened(IDialogParameters parameters) {
        }
    }
}
