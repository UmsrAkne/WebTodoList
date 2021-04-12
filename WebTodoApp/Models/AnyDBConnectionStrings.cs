using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    class AnyDBConnectionStrings : BindableBase, IDBConnectionStrings{
        public string UserName { get => userName; set => SetProperty(ref userName, value); }
        private string userName = "";

        public string PassWord { get => password; set => SetProperty(ref password, value); }
        private string password = "";

        public string HostName { get => hostName; set => SetProperty(ref hostName, value); }
        private string hostName = "";

        public int PortNumber { get => portNumber; set => SetProperty(ref portNumber, value); }
        private int portNumber = 5432;

        public string ServiceName { get; set; }
    }
}
