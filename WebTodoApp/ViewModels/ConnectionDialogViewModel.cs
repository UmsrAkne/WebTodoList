using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.ViewModels {
    class ConnectionDialogViewModel : IDialogAware {
        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() {
            throw new NotImplementedException();
        }

        public void OnDialogClosed() {
            throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            throw new NotImplementedException();
        }
    }
}
