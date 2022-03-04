namespace WebTodoApp.Models
{
    using System.IO;
    using Prism.Mvvm;

    public class AnyDBConnectionStrings : BindableBase, IDBConnectionStrings
    {
        private string userName = string.Empty;
        private string password = string.Empty;
        private string hostName = string.Empty;
        private int portNumber = 5432;

        public AnyDBConnectionStrings()
        {
        }

        /// <summary>
        /// 認証情報が記載された暗号化済みファイルを復号し、各プロパティを入力します。
        /// </summary>
        /// <param name="certificationFilePath"></param>
        public AnyDBConnectionStrings(string certificationFilePath)
        {
            var certificationFileInfo = new FileInfo(certificationFilePath);
            var encryptor = new Encryptor();

            if (certificationFileInfo.Exists)
            {
                using (var sr = new StreamReader(certificationFileInfo.Name))
                {
                    var encString = sr.ReadToEnd();
                    var decString = encryptor.Decrypt(encString);
                    var strings = decString.Split(' ');

                    HostName = strings[0];
                    UserName = strings[1];
                    PassWord = strings[2];
                    PortNumber = int.Parse(strings[3]);
                }
            }
        }

        public string UserName { get => userName; set => SetProperty(ref userName, value); }

        public string PassWord { get => password; set => SetProperty(ref password, value); }

        public string HostName { get => hostName; set => SetProperty(ref hostName, value); }

        public int PortNumber { get => portNumber; set => SetProperty(ref portNumber, value); }

        public string ServiceName { get; set; }
    }
}
