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

namespace WebTodoApp.ViewModels
{
    class ConnectionDialogViewModel : IDialogAware
    {
        public string Title => "サーバーの情報を入力";

        public event Action<IDialogResult> RequestClose;

        public AnyDBConnectionStrings DBConnectionStrings { get; } = new AnyDBConnectionStrings();

        public bool CanCloseDialog() => true;

        private DBHelper dbHelper;
        private Encryptor encryptor = new Encryptor();

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public ConnectionDialogViewModel()
        {
            DBConnectionStrings = new AnyDBConnectionStrings("certification");
        }

        private void saveCertification()
        {
            string encrypted = encryptor.encrypt($"{DBConnectionStrings.HostName} {DBConnectionStrings.UserName} {DBConnectionStrings.PassWord} {DBConnectionStrings.PortNumber}");

            var certificationFileInfo = new FileInfo("certification");
            using (StreamWriter sw = new StreamWriter(certificationFileInfo.Name, false))
            {
                sw.Write(encrypted);
            }
        }

        public DelegateCommand ConnectCommand
        {
            #region
            get => connectCommand ?? (connectCommand = new DelegateCommand(() =>
            {
                saveCertification();

                DialogParameters dp = new DialogParameters();
                dp.Add(nameof(AnyDBConnectionStrings), DBConnectionStrings);
                var result = new DialogResult(ButtonResult.Yes, dp);
                RequestClose.Invoke(result);
            }));
        }
        private DelegateCommand connectCommand;
        #endregion

        public DelegateCommand CancelDialogCommand
        {
            #region
            get => cancelDialogCommand ?? (cancelDialogCommand = new DelegateCommand(() =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
            }));
        }
        private DelegateCommand cancelDialogCommand;
        #endregion

    }
}
