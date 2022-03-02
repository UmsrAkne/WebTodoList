namespace WebTodoApp.ViewModels
{
    using System;
    using System.IO;
    using Prism.Commands;
    using Prism.Services.Dialogs;
    using WebTodoApp.Models;

    class ConnectionDialogViewModel : IDialogAware
    {
        public string Title => "サーバーの情報を入力";

        public event Action<IDialogResult> RequestClose;

        public AnyDBConnectionStrings DBConnectionStrings { get; } = new AnyDBConnectionStrings();

        public bool CanCloseDialog() => true;

        private DBHelper dbHelper;
        private Encryptor encryptor = new Encryptor();
        private DelegateCommand connectCommand;
        private DelegateCommand cancelDialogCommand;

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
            get => connectCommand ?? (connectCommand = new DelegateCommand(() =>
            {
                saveCertification();

                DialogParameters dp = new DialogParameters();
                dp.Add(nameof(AnyDBConnectionStrings), DBConnectionStrings);
                var result = new DialogResult(ButtonResult.Yes, dp);
                RequestClose.Invoke(result);
            }));
        }

        public DelegateCommand CancelDialogCommand
        {
            get => cancelDialogCommand ?? (cancelDialogCommand = new DelegateCommand(() =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
            }));
        }
    }
}
