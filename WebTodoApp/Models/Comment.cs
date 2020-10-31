using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    public class Comment {
        public int ID { get; set; }
        public DateTime CreationDateTime { get; set; }
        public String TextContent { get; set; }
    }
}
