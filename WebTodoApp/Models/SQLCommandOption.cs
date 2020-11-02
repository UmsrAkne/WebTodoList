using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    public class SQLCommandOption {

        public int Limit { get; set; } = 10;
        public string[] OrderByColumn { get; set; } = new string[0];

        /// <summary>
        /// true にした場合、結果を降順で並べます。
        /// </summary>
        public bool DESC { get; set; }
    }
}
