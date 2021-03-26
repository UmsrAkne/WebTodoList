using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    public interface IDBConnectionStrings {
        string UserName { get; }

        string PassWord { get; }

        string HostName { get; }

        int PortNumber { get; }

        string ServiceName { get; }
    }
}
