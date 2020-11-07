using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Npgsql;
using Prism.Commands;
using Prism.Mvvm;

namespace WebTodoApp.Models
{
    public class DBHelper : BindableBase
    {

        private NpgsqlConnectionStringBuilder connectionStringBuilder;

        public List<Todo> TodoList { get => todoList; private set => SetProperty(ref todoList, value); }
        private List<Todo> todoList = new List<Todo>();

        public DBHelper(string tableName) {

            string userName;
            using (StreamReader sr = new StreamReader(
                @"C:\Users\main10\awsrds\user.txt", Encoding.GetEncoding("Shift_JIS"))) {
                userName = sr.ReadToEnd();
            }

            string pass;
            using (StreamReader sr = new StreamReader(
                @"C:\Users\main10\awsrds\pass.txt", Encoding.GetEncoding("Shift_JIS"))) {
                pass = sr.ReadToEnd();
            }

            string hostName;
            using (StreamReader sr = new StreamReader(
                @"C:\Users\main10\awsrds\hostName.txt", Encoding.GetEncoding("Shift_JIS"))) {
                hostName = sr.ReadToEnd();
            }

            int portNumber;
            using (StreamReader sr = new StreamReader(
                @"C:\Users\main10\awsrds\port.txt", Encoding.GetEncoding("Shift_JIS"))) {
                portNumber = int.Parse(sr.ReadToEnd());
            }

            connectionStringBuilder = new NpgsqlConnectionStringBuilder() {
                Host = hostName,
                Username = userName,
                Database = "postgres",
                Password = pass,
                Port = portNumber
            };

            TableName = tableName;
            //createTable();

            SqlCommandOption = new SQLCommandOption();
            SqlCommandOption.Limit = 100;
            SqlCommandOption.TableName = TableName;
            SqlCommandOption.OrderByColumns.Add(
                new SQLCommandOption.SQLCommandColumnOption() {
                    Name = nameof(Todo.CreationDate),
                    DESC = true
                }
            );

            TryFirstConnectCommand.Execute();
        }

        public void insertTodo(Todo todo) {
            var maxIDRow = select($"SELECT MAX ({nameof(Todo.ID)}) FROM {TableName};")[0];
            var maxID = (int)maxIDRow["max"] + 1;

            executeNonQuery(
                $"INSERT INTO {TableName} ( " +
                $"{nameof(Todo.ID)}, " +
                $"{nameof(Todo.Completed)}, " +
                $"{nameof(Todo.Title)}, " +
                $"{nameof(Todo.TextContent)}," +
                $"{nameof(Todo.CreationDate)}," +
                $"{nameof(Todo.CompletionDate)}," +
                $"{nameof(Todo.StartDateTime)}," +
                $"{nameof(Todo.Priority)}," +
                $"{nameof(Todo.Duration)}," +
                $"{nameof(Todo.Tag)} ) " +
                $"VALUES (" +
                $"{maxID}," +
                $"{todo.Completed}," +
                $"'{todo.Title}'," +
                $"'{todo.TextContent}'," +
                $"'{todo.CreationDate}'," +
                $"'{todo.CompletionDate}'," +
                $"'{todo.StartDateTime}'," +
                $"{todo.Priority}," +
                $"{todo.Duration}," +
                $"'{todo.Tag}'" +
                ");"
            );

            RaisePropertyChanged(nameof(TodoCount));
            loadTodoList();
        }

        public void insertComment(Comment comment) {
            var maxIDRow = select($"SELECT MAX ({nameof(Comment.ID)}) FROM {CommentTableName};")[0];
            int maxID = (maxIDRow["max"] is System.DBNull) ? 1 : (int)maxIDRow["max"] + 1;

            executeNonQuery(
                $"INSERT INTO {CommentTableName} (" +
                $"{nameof(Comment.ID)}, " +
                $"{nameof(Comment.CreationDateTime)}," +
                $"{nameof(Comment.TextContent)} )" +
                $"VALUES (" +
                $"{maxID}," +
                $"'{comment.CreationDateTime}', " +
                $"'{comment.TextContent}' " +
                $");"
            );

        }

        public void update(Todo todo) {
            System.Diagnostics.Debug.WriteLine(todo);
            if(todo.ID < 0 || !todo.existSource) {
                // 上記を満たす場合、更新すべきTodoのソース（行）が存在しない
                throw new ArgumentException("更新すべき行を特定できないTodoが指定されました。");
            }

            if (todo.updateStopping) {
                return;
            }

            executeNonQuery(
                $"update {TableName} SET " +
                $"{nameof(Todo.Completed)} = {todo.Completed}, " +
                $"{nameof(Todo.Title)} = '{todo.Title}', " +
                $"{nameof(Todo.TextContent)} = '{todo.TextContent}', " +
                $"{nameof(Todo.CreationDate)} = '{todo.CreationDate}', " +
                $"{nameof(Todo.CompletionDate)} = '{todo.CompletionDate}', " +
                $"{nameof(Todo.StartDateTime)} = '{todo.StartDateTime}', " +
                $"{nameof(Todo.Priority)} = {todo.Priority}, " +
                $"{nameof(Todo.Duration)} = {todo.Duration}, " +
                $"{nameof(Todo.Tag)} = '{todo.Tag}' " +
                $"WHERE id = {todo.ID};"
            );
        }

        public DelegateCommand<object> UpdateCommand {
            #region
            get => updateCommand ?? (updateCommand = new DelegateCommand<object>((object l) => {
                Todo todo = ((ListViewItem)l).Content as Todo;
                if(todo != null) {
                    update(todo);
                }
            }));
        }
        private DelegateCommand<object> updateCommand;
        #endregion

        /// <summary>
        /// select で取得した dataReader から取り出した値を Hashtable に詰め、それをまとめたリストを取得します。
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        private List<Hashtable> select(string commandText) {
                var startTime = DateTime.Now;

            using (var con = DBConnection) {
                List<Hashtable> resultList = new List<Hashtable>();
                con.Open();
                var command = new NpgsqlCommand(commandText, con);
                var dataReader = command.ExecuteReader();

                while (dataReader.Read()) {
                    var hashtable = new Hashtable();
                    for(int i = 0; i < dataReader.FieldCount; i++) {
                        hashtable[dataReader.GetName(i)] = dataReader.GetValue(i);
                    }
                    resultList.Add(hashtable);
                }

                TimeSpan processDuration = DateTime.Now - startTime;
                File.AppendAllText(@"sqllog.txt", $"{DateTime.Now} {processDuration}\t{commandText}{Environment.NewLine}");

                return resultList;
            };
        }

        private void executeNonQuery(string CommandText) {
            var startTime = DateTime.Now;

            using (var con = DBConnection) {
                con.Open();
                var Command = new NpgsqlCommand(CommandText, con);
                Command.ExecuteNonQuery();
            }

            TimeSpan processDuration = DateTime.Now - startTime;
            File.AppendAllText(@"sqllog.txt", $"{DateTime.Now} {processDuration}\t{CommandText}{Environment.NewLine}");
        }

        /// <summary>
        /// if not exists を含むテーブル生成用の sql文 を実行します。
        /// </summary>
        private void createTable() {
            executeNonQuery(
                $"CREATE TABLE IF NOT EXISTS {TableName} (" +
                $"id INTEGER PRIMARY KEY, " +
                $"{nameof(Todo.Completed)} BOOLEAN NOT NULL, " +
                $"{nameof(Todo.Title)} TEXT NOT NULL, " +
                $"{nameof(Todo.TextContent)} TEXT NOT NULL, " +
                $"{nameof(Todo.CreationDate)} TIMESTAMP NOT NULL, " +
                $"{nameof(Todo.CompletionDate)} TIMESTAMP NOT NULL, " +
                $"{nameof(Todo.Priority)} INTEGER NOT NULL, " +
                $"{nameof(Todo.Duration)} INTEGER DEFAULT 0 NOT NULL, " +
                $"{nameof(Todo.StartDateTime)} TIMESTAMP NOT NULL DEFAULT '0001/01/01 0:00:00', " +
                $"{nameof(Todo.Tag)} TEXT NOT NULL " +
                ");"
            );

            executeNonQuery(
                $"CREATE TABLE IF NOT EXISTS {CommentTableName} (" +
                $"{nameof(Comment.ID)} INTEGER PRIMARY KEY, " +
                $"{nameof(Comment.CreationDateTime)} TIMESTAMP NOT NULL," +
                $"{nameof(Comment.TextContent)} TEXT NOT NULL " +
                $");"
            );
        }

        public string TableName { get; private set; }
        public string CommentTableName { get; private set; } = "comments";

        private NpgsqlConnection DBConnection {
            get => new NpgsqlConnection(connectionStringBuilder.ToString());
        }

        private void loadTodoList() {
            var rows = select(SqlCommandOption.buildSQL());
            var list = new List<Todo>();
            rows.ForEach((Hashtable row) => {
                list.Add(toTodo(row));
            });

            TodoList = list;
        }

        private Todo toTodo(Hashtable hashtable) {
            Todo todo = new Todo();

            todo.existSource = true;

            todo.ID = (int)hashtable[nameof(Todo.ID).ToLower()];
            todo.Completed = (Boolean)hashtable[nameof(Todo.Completed).ToLower()];
            todo.Title = (string)hashtable[nameof(Todo.Title).ToLower()];
            todo.TextContent = (string)hashtable[nameof(Todo.TextContent).ToLower()];

            todo.CreationDate = (DateTime)hashtable[nameof(Todo.CreationDate).ToLower()];
            todo.CompletionDate = (DateTime)hashtable[nameof(Todo.CompletionDate).ToLower()];
            todo.StartDateTime = (DateTime)hashtable[nameof(Todo.StartDateTime).ToLower()];

            todo.Priority = (int)hashtable[nameof(Todo.Priority).ToLower()];
            todo.Duration = (int)hashtable[nameof(Todo.Duration).ToLower()];
            todo.Tag = (String)hashtable[nameof(Todo.Tag).ToLower()];

            return todo;
        }

        public DelegateCommand TryFirstConnectCommand {
            get => tryFirstConnectCommand ?? (tryFirstConnectCommand = new DelegateCommand(() => {
                try {
                    loadTodoList();
                    Message = $"{DateTime.Now} データベースへの接続に成功。TodoList をロードしました";
                }
                catch (TimeoutException) {
                    Message = $"{DateTime.Now} 接続を試行しましたがタイムアウトしました。データベースへの接続に失敗しました";
                }
            }));
        }
        private DelegateCommand tryFirstConnectCommand;


        public DelegateCommand LoadCommand {
            #region
            get => loadCommand ?? (loadCommand = new DelegateCommand(() => {
                loadTodoList();
                Message = $"{DateTime.Now} TodoList をリロードしました。";
            }));
        }
        private DelegateCommand loadCommand;
        #endregion


        public String Message { get => message; set => SetProperty(ref message, value); }
        private String message = "";

        public long TodoCount { get => (long)(select($"SELECT COUNT(*) FROM {TableName};")[0]["count"]); }

        public SQLCommandOption SqlCommandOption { get; private set; }
    }
}
