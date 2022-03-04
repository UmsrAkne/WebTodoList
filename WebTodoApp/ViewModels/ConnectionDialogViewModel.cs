namespace WebTodoApp.ViewModels
{
    using System;
    using System.IO;
    using Prism.Commands;
    using Prism.Services.Dialogs;
    using WebTodoApp.Models;

    public class ConnectionDialogViewModel : IDialogAware
    {
        private Encryptor encryptor = new Encryptor();
        private DelegateCommand connectCommand;
        private DelegateCommand cancelDialogCommand;

        public ConnectionDialogViewModel()
        {
            DBConnectionStrings = new AnyDBConnectionStrings("certification");
        }

        public event Action<IDialogResult> RequestClose;

        public AnyDBConnectionStrings DBConnectionStrings { get; } = new AnyDBConnectionStrings();

        public string Title => "サーバーの情報を入力";

        public DelegateCommand ConnectCommand
        {
            get => connectCommand ?? (connectCommand = new DelegateCommand(() =>
            {
                SaveCertification();

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

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        private void SaveCertification()
        {
            string encrypted = encryptor.Encrypt($"{DBConnectionStrings.HostName} {DBConnectionStrings.UserName} {DBConnectionStrings.PassWord} {DBConnectionStrings.PortNumber}");

            var certificationFileInfo = new FileInfo("certification");
            using (StreamWriter sw = new StreamWriter(certificationFileInfo.Name, false))
            {
                sw.Write(encrypted);
            }
        }
    }
}
