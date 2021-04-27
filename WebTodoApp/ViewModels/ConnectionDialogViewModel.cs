using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebTodoApp.Models;

namespace WebTodoApp.ViewModels {
    class ConnectionDialogViewModel : IDialogAware {
        public string Title => "サーバーの情報を入力";

        public event Action<IDialogResult> RequestClose;

        public AnyDBConnectionStrings DBConnectionStrings { get; } = new AnyDBConnectionStrings();

        public bool CanCloseDialog() => true;

        private DBHelper dbHelper = new DBHelper("todo_table", DBServerName.EC2);
        private Encryptor encryptor = new Encryptor();

        public void OnDialogClosed() {
        }

        public void OnDialogOpened(IDialogParameters parameters) {
        }

        public ConnectionDialogViewModel() {
            var certificationFileInfo = new FileInfo("certification");

            if (certificationFileInfo.Exists) {
                using (var sr = new StreamReader(certificationFileInfo.Name)) {
                    var encString = sr.ReadToEnd();
                    var decString = encryptor.decrypt(encString);
                    var cnStrings = decString.Split(' ');

                    DBConnectionStrings.HostName = cnStrings[0];
                    DBConnectionStrings.UserName = cnStrings[1];
                    DBConnectionStrings.PassWord = cnStrings[2];
                    DBConnectionStrings.PortNumber = int.Parse(cnStrings[3]);
                }
            }
        }

        private void saveCertification() {
            string encrypted = encryptor.encrypt($"{DBConnectionStrings.HostName} {DBConnectionStrings.UserName} {DBConnectionStrings.PassWord} {DBConnectionStrings.PortNumber}");

            var certificationFileInfo = new FileInfo("certification");
            using (StreamWriter sw = new StreamWriter(certificationFileInfo.Name, false)) {
                sw.Write(encrypted);
            }
        }

        public DelegateCommand ConnectCommand {
            #region
            get => connectCommand ?? (connectCommand = new DelegateCommand(() => {
                saveCertification();
                dbHelper.changeDatabase(DBConnectionStrings);
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
