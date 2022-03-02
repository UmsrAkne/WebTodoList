namespace WebTodoApp.Models
{
    using System.IO;
    using Prism.Mvvm;

    public class AnyDBConnectionStrings : BindableBase, IDBConnectionStrings
    {
        public string UserName { get => userName; set => SetProperty(ref userName, value); }
        private string userName = string.Empty;

        public string PassWord { get => password; set => SetProperty(ref password, value); }
        private string password = string.Empty;

        public string HostName { get => hostName; set => SetProperty(ref hostName, value); }
        private string hostName = string.Empty;

        public int PortNumber { get => portNumber; set => SetProperty(ref portNumber, value); }
        private int portNumber = 5432;

        public string ServiceName { get; set; }

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
                    var decString = encryptor.decrypt(encString);
                    var cnStrings = decString.Split(' ');

                    HostName = cnStrings[0];
                    UserName = cnStrings[1];
                    PassWord = cnStrings[2];
                    PortNumber = int.Parse(cnStrings[3]);
                }
            }

        }
    }
}
