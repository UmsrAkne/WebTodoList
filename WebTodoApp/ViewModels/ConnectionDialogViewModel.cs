using Prism.Commands;
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

        private DBHelper dbHelper = new DBHelper("todo_table", DBServerName.EC2);

        public void OnDialogClosed() {
        }

        public void OnDialogOpened(IDialogParameters parameters) {
        }

        public DelegateCommand ConnectCommand {
            #region
            get => connectCommand ?? (connectCommand = new DelegateCommand(() => {
                dbHelper.tryConnect();
            }));
        }
        private DelegateCommand connectCommand;
        #endregion

        public DelegateCommand CancelDialogCommand
            {
            #region
            get => cancelDialogCommand ?? (cancelDialogCommand = new DelegateCommand(() => {
                RequestClose.Invoke(null);
            }));
        }
        private DelegateCommand cancelDialogCommand;
        #endregion

    }
}
