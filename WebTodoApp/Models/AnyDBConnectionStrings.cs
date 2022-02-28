using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models
{
    class AnyDBConnectionStrings : BindableBase, IDBConnectionStrings
    {
        public string UserName { get => userName; set => SetProperty(ref userName, value); }
        private string userName = "";

        public string PassWord { get => password; set => SetProperty(ref password, value); }
        private string password = "";

        public string HostName { get => hostName; set => SetProperty(ref hostName, value); }
        private string hostName = "";

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
