using System;
using System.IO;
using System.Text;

namespace WebTodoApp.Models {
    class RDSConnectionStrings : IDBConnectionStrings {

        public string UserName { get; private set; }

        public string PassWord { get; private set; }

        public string HostName  { get; private set; }

        public int PortNumber { get; private set; }

        public string ServiceName => "RDS";

        public RDSConnectionStrings() {

            string homePath =
                Environment.GetEnvironmentVariable("HOMEDRIVE") +
                Environment.GetEnvironmentVariable("HOMEPATH");

            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\user.txt", Encoding.GetEncoding("Shift_JIS"))) {
                UserName = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\pass.txt", Encoding.GetEncoding("Shift_JIS"))) {
                PassWord = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\hostName.txt", Encoding.GetEncoding("Shift_JIS"))) {
                HostName = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\port.txt", Encoding.GetEncoding("Shift_JIS"))) {
                PortNumber = int.Parse(sr.ReadToEnd());
            }

        }
    }
}
