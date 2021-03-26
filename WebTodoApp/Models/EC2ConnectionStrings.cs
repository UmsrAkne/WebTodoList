using System;
using System.IO;
using System.Text;

namespace WebTodoApp.Models {
    class EC2ConnectionStrings : IDBConnectionStrings {
        public string UserName { get; private set; }

        public string PassWord { get; private set; }

        public string HostName { get; private set; }

        public int PortNumber { get; private set; }

        public string ServiceName => "EC2";

        public EC2ConnectionStrings() {

            string basePath =
                Environment.GetEnvironmentVariable("HOMEDRIVE") +
                Environment.GetEnvironmentVariable("HOMEPATH") + 
                @"\ec2db\";

            using (StreamReader sr = new StreamReader(
                basePath + "user.txt", Encoding.GetEncoding("Shift_JIS"))) {
                UserName = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(
                basePath + "pass.txt", Encoding.GetEncoding("Shift_JIS"))) {
                PassWord = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(
                basePath + "hostName.txt", Encoding.GetEncoding("Shift_JIS"))) {
                HostName = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(
                basePath + "port.txt", Encoding.GetEncoding("Shift_JIS"))) {
                PortNumber = int.Parse(sr.ReadToEnd());
            }

        }
    }
}
