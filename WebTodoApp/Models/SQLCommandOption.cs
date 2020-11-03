using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    public class SQLCommandOption {

        public string TableName { get; set; }

        public int Limit { get; set; } = 10;
        public List<SQLCommandColumnOption> OrderByColumns { get; set; } = new List<SQLCommandColumnOption>();

        /// <summary>
        /// 取得する Todo を未完了の Todo のみに絞るかどうかを設定します。
        /// </summary>
        public bool ShowOnlyIncompleteTodo { get; set; } 

        public string buildSQL() {
            var sql = $"select * from {TableName} ";

            if (ShowOnlyIncompleteTodo) {
                sql += $"where {nameof(Todo.Completed)} = false ";
            }

            if(OrderByColumns.Count > 0) {
                sql += $"order by ";
                OrderByColumns.ForEach((SQLCommandColumnOption cco) => {
                    sql += $"{cco.Name} ";
                    sql += (cco.DESC) ? "DESC ," : "ASC ,";
                });

                sql = sql.Substring(0, sql.Length - 2); // 終端についているカンマを削除する
            }

            sql += $" LIMIT {Limit}";
            sql += ";";

            return sql;
        }

        public class SQLCommandColumnOption {
            public string Name { get; set; }
            public bool DESC { get; set; }
        }
    }
}
