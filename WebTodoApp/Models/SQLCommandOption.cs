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

            sql += "where 1=1 ";

            if (ShowOnlyIncompleteTodo) {
                sql += $"AND {nameof(Todo.Completed)} = false ";
            }

            if(DisplayDateRange > 0) {
                var pastDate = DateTime.Now - new TimeSpan(24 * DisplayDateRange, 0, 0);
                sql += $"AND {nameof(Todo.CreationDate)} >= '{pastDate}' ";
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

        /// <summary>
        /// このプロパティにセットされた整数日前から Todo を表示するよう設定します。
        /// 例えば 1 をセットした場合、その時の日付から、作成日時が一日前までの Todo を検索するSQLを生成します。
        /// デフォルトは 0 となっており、この場合は全ての期間の Todo を指定します。
        /// </summary>
        public int DisplayDateRange { get; set; }

        public string DisplayDateRangeString {
            get => displayDateRangeString;
            set {
                if(int.TryParse(value,out int result)) {
                    DisplayDateRange = result;
                }
                else {
                    DisplayDateRange = 0;
                }

                displayDateRangeString = DisplayDateRange.ToString();
            }
        }

        private string displayDateRangeString = "0";

        public class SQLCommandColumnOption {
            public string Name { get; set; }
            public bool DESC { get; set; }
        }
    }
}
