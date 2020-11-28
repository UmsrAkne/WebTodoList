﻿using System;
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

        public List<Comment> CommentList { get => commentList; private set => SetProperty(ref commentList, value); }
        private List<Comment> commentList = new List<Comment>();

        public DBHelper(string tableName) {

            string homePath =
                Environment.GetEnvironmentVariable("HOMEDRIVE") +
                Environment.GetEnvironmentVariable("HOMEPATH");

            string userName;
            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\user.txt", Encoding.GetEncoding("Shift_JIS"))) {
                userName = sr.ReadToEnd();
            }

            string pass;
            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\pass.txt", Encoding.GetEncoding("Shift_JIS"))) {
                pass = sr.ReadToEnd();
            }

            string hostName;
            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\hostName.txt", Encoding.GetEncoding("Shift_JIS"))) {
                hostName = sr.ReadToEnd();
            }

            int portNumber;
            using (StreamReader sr = new StreamReader(
                homePath + @"\awsrds\port.txt", Encoding.GetEncoding("Shift_JIS"))) {
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
            var maxIDRow = select(
                $"SELECT MAX ({nameof(Todo.ID)}) FROM {TableName};",
                new List<NpgsqlParameter>()
                )[0];

            var maxID = (int)maxIDRow["max"] + 1;

            var ps = new List<NpgsqlParameter>();

            // ユーザーの入力が入る余地がある部分はパラメーターによる入力を行う。
            // もともと固定になっている部分はそうでなくてもOKなはず
            ps.Add(new NpgsqlParameter(nameof(Todo.Title), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Title });
            ps.Add(new NpgsqlParameter(nameof(Todo.TextContent), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.TextContent });
            ps.Add(new NpgsqlParameter(nameof(Todo.Priority), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Priority });
            ps.Add(new NpgsqlParameter(nameof(Todo.Duration), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Duration });
            ps.Add(new NpgsqlParameter(nameof(Todo.Tag), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Tag });

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
                $":{nameof(todo.Title)}," +
                $":{nameof(todo.TextContent)}," +
                $"'{todo.CreationDate}'," +
                $"'{todo.CompletionDate}'," +
                $"'{todo.StartDateTime}'," +
                $":{nameof(todo.Priority)}," +
                $":{nameof(todo.Duration)}," +
                $":{nameof(todo.Tag)}" +
                ");"
                , ps
            );

            RaisePropertyChanged(nameof(TodoCount));
            loadTodoList();
        }

        public void insertComment(Comment comment) {
            var maxIDRow = select(
                $"SELECT MAX ({nameof(Comment.ID)}) FROM {CommentTableName};",
                new List<NpgsqlParameter>()
                )[0];

            int maxID = (maxIDRow["max"] is System.DBNull) ? 1 : (int)maxIDRow["max"] + 1;

            var ps = new List<NpgsqlParameter>();
            ps.Add(new NpgsqlParameter(nameof(Comment.TextContent), NpgsqlTypes.NpgsqlDbType.Text) { Value = comment.TextContent });

            executeNonQuery(
                $"INSERT INTO {CommentTableName} (" +
                $"{nameof(Comment.ID)}, " +
                $"{nameof(Comment.CreationDateTime)}," +
                $"{nameof(Comment.TextContent)} )" +
                $"VALUES (" +
                $"{maxID}," +
                $"'{comment.CreationDateTime}', " +
                $":{nameof(comment.TextContent)} " +
                $");"
                ,ps
            );

            loadCommentList();
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

            var ps = new List<NpgsqlParameter>();

            ps.Add(new NpgsqlParameter(nameof(Todo.Title), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Title });
            ps.Add(new NpgsqlParameter(nameof(Todo.TextContent), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.TextContent });
            ps.Add(new NpgsqlParameter(nameof(Todo.Priority), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Priority });
            ps.Add(new NpgsqlParameter(nameof(Todo.Duration), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Duration });
            ps.Add(new NpgsqlParameter(nameof(Todo.Tag), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Tag });

            executeNonQuery(
                $"update {TableName} SET " +
                $"{nameof(Todo.Completed)} = {todo.Completed}, " +
                $"{nameof(Todo.Title)} = :{nameof(todo.Title)}, " +
                $"{nameof(Todo.TextContent)} = :{nameof(todo.TextContent)}, " +
                $"{nameof(Todo.CreationDate)} = '{todo.CreationDate}', " +
                $"{nameof(Todo.CompletionDate)} = '{todo.CompletionDate}', " +
                $"{nameof(Todo.StartDateTime)} = '{todo.StartDateTime}', " +
                $"{nameof(Todo.Priority)} = :{nameof(todo.Priority)}, " +
                $"{nameof(Todo.Duration)} = :{nameof(todo.Duration)}, " +
                $"{nameof(Todo.Tag)} = :{nameof(todo.Tag)} " +
                $"WHERE id = {todo.ID};"
                ,ps
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
        private List<Hashtable> select(string commandText, List<NpgsqlParameter> parameters) {
                var startTime = DateTime.Now;

            using (var con = DBConnection) {
                List<Hashtable> resultList = new List<Hashtable>();
                con.Open();
                var command = new NpgsqlCommand(commandText, con);
                parameters.ForEach(p => command.Parameters.Add(p));
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

        private void executeNonQuery(string CommandText, List<NpgsqlParameter> commandParams) {
            var startTime = DateTime.Now;

            using (var con = DBConnection) {
                con.Open();
                var Command = new NpgsqlCommand(CommandText, con);
                commandParams.ForEach((param) => { Command.Parameters.Add(param); });
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
                , new List<NpgsqlParameter>()
            );

            executeNonQuery(
                $"CREATE TABLE IF NOT EXISTS {CommentTableName} (" +
                $"{nameof(Comment.ID)} INTEGER PRIMARY KEY, " +
                $"{nameof(Comment.CreationDateTime)} TIMESTAMP NOT NULL," +
                $"{nameof(Comment.TextContent)} TEXT NOT NULL " +
                $");"
                , new List<NpgsqlParameter>()
            );
        }

        public string TableName { get; private set; }
        public string CommentTableName { get; private set; } = "comments";

        private NpgsqlConnection DBConnection {
            get => new NpgsqlConnection(connectionStringBuilder.ToString());
        }

        private void loadTodoList() {
            var rows = select(
                SqlCommandOption.buildSQL(),
                SqlCommandOption.SqlParams
                );
            var list = new List<Todo>();
            rows.ForEach((Hashtable row) => {
                list.Add(toTodo(row));
            });

            TodoList = list;
        }

        private void loadCommentList() {
            var rows = select(
                $"SELECT * FROM {CommentTableName} ORDER BY {nameof(Comment.CreationDateTime)} DESC;",
                new List<NpgsqlParameter>()
                );

            var list = new List<Comment>();

            rows.ForEach((Hashtable row) => {
                list.Add(new Comment() {
                    CreationDateTime = (DateTime)row[nameof(Comment.CreationDateTime).ToLower()],
                    TextContent = (String)row[nameof(Comment.TextContent).ToLower()],
                    ID = (int)row[nameof(Comment.ID).ToLower()],
                });
            });

            CommentList = list;
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

            if(todo.CompletionDate.Ticks != 0) {
                // 既に完了している状態
                todo.Started = false;
                todo.CanStart = false;
            }
            else {
                // todo 未完了の状態
                todo.Started = (todo.StartDateTime.Ticks != 0);
                todo.CanStart = (todo.CompletionDate.Ticks == 0 && todo.StartDateTime.Ticks == 0);
            }

            if(todo.CompletionDate.Ticks != 0 && todo.StartDateTime.Ticks != 0) {
                todo.ActualDuration = (int)(todo.CompletionDate - todo.StartDateTime).TotalMinutes;
            }

            todo.Priority = (int)hashtable[nameof(Todo.Priority).ToLower()];
            todo.Duration = (int)hashtable[nameof(Todo.Duration).ToLower()];
            todo.Tag = (String)hashtable[nameof(Todo.Tag).ToLower()];

            return todo;
        }

        public DelegateCommand TryFirstConnectCommand {
            get => tryFirstConnectCommand ?? (tryFirstConnectCommand = new DelegateCommand(() => {
                try {
                    loadTodoList();
                    loadCommentList();
                    Message = "データベースへの接続に成功。TodoList をロードしました";
                }
                catch (TimeoutException) {
                    Message = "接続を試行しましたがタイムアウトしました。データベースへの接続に失敗しました";
                }
            }));
        }
        private DelegateCommand tryFirstConnectCommand;


        public DelegateCommand LoadCommand {
            #region
            get => loadCommand ?? (loadCommand = new DelegateCommand(() => {
                loadTodoList();
                Message = "TodoList をリロードしました。";
            }));
        }
        private DelegateCommand loadCommand;
        #endregion


        public DelegateCommand<Todo> CopyTodoCommand {
            #region
            get => copyTodoCommand ?? (copyTodoCommand = new DelegateCommand<Todo>((sourceTodo) => {
                insertTodo(new Todo(sourceTodo));
            }));
        }
        private DelegateCommand<Todo> copyTodoCommand;
        #endregion


        public DelegateCommand<Todo> CopyTodoWithoutTextCommand {
            #region
            get => copyTodoWithoutTextCommand ?? (copyTodoWithoutTextCommand = new DelegateCommand<Todo>((sourceTodo) => {
                Todo t = new Todo(sourceTodo) { TextContent = "" };
                insertTodo(t);
            }));
        }
        private DelegateCommand<Todo> copyTodoWithoutTextCommand;
        #endregion


        public DelegateCommand<Todo> ResetTodoWorkingStatusCommand {
            #region
            get => resetTodoWorkingStatusCommand ?? (resetTodoWorkingStatusCommand = new DelegateCommand<Todo>((targetTodo) => {
                targetTodo.resetWorkingStatus();
                update(targetTodo);
            }));
        }
        private DelegateCommand<Todo> resetTodoWorkingStatusCommand;
        #endregion


        public String Message {
            get => message;
            set {
                value = $"{DateTime.Now} " + value;
                SetProperty(ref message, value);
            }
        }
        private String message = "";

        public long TodoCount {
            get {
                return (long)(select(
                    $"SELECT COUNT(*) FROM {TableName};",
                    new List<NpgsqlParameter>()
                    )[0]["count"]);
            }
        }

        public SQLCommandOption SqlCommandOption { get; private set; }
    }
}
