using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Media;
using Npgsql;
using Prism.Commands;
using Prism.Mvvm;
using System.Net.Sockets;

namespace WebTodoApp.Models
{
    public class DBHelper : BindableBase
    {

        private NpgsqlConnectionStringBuilder connectionStringBuilder;

        public List<Todo> TodoList { get => todoList; private set => SetProperty(ref todoList, value); }
        private List<Todo> todoList = new List<Todo>();

        public HashSet<Todo> WorkingTodos { get; set; } = new HashSet<Todo>();
        private Timer timer = new Timer(10000);

        public bool Connected { get; set; } = false;

        private SoundPlayer soundPlayer = new SoundPlayer(@"C:\Windows\Media\Windows Notify Calendar.wav");
        private string CurrentServiceName { get; set; }

        private DBHelper(string tableName) {
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

            timer.Elapsed += (sender, e) => {
                foreach(Todo t in WorkingTodos) {
                    t.updateElapsedTime();

                    if((int)t.Duration == (int)(DateTime.Now - t.StartDateTime).TotalMinutes && !t.Completed) {
                        soundPlayer.Play();
                    }
                }
            };

            timer.Start();

        }

        public DBHelper(string tableName, IDBConnectionStrings dbConnectionStrings) : this(tableName){
            changeDatabase(dbConnectionStrings);
        }

        public void insertTodo(Todo todo) {
            var count = select(
                $"SELECT COUNT (*) FROM {TableName};",
                new List<NpgsqlParameter>()
                )[0];

            int maxID = 0;

            if((long)count["count"] > 0) {
                var maxIDRow = select(
                    $"SELECT MAX ({nameof(Todo.ID)}) FROM {TableName};",
                    new List<NpgsqlParameter>()
                    )[0];

                maxID = (int)maxIDRow["max"] + 1;
            };


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
                $"{nameof(Todo.LabelColor)}," +
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
                $"'{todo.LabelColorName}'," +
                $":{nameof(todo.Tag)}" +
                ");"
                , ps
            );

            RaisePropertyChanged(nameof(TodoCount));
            loadTodoList();
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
                $"{nameof(Todo.Tag)} = :{nameof(todo.Tag)}, " +
                $"{nameof(Todo.LabelColor)} = '{todo.LabelColorName}' " +
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
                    if (todo.Started) {
                        WorkingTodos.Add(todo);
                    }
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
                $"{nameof(Todo.LabelColor)} TEXT NOT NULL, " +
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

            WorkingTodos.Clear();

            rows.ForEach((Hashtable row) => {
                var t = toTodo(row);

                if (t.Started) {
                    WorkingTodos.Add(t);
                }

                list.Add(t);
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

            string labelColor = (String)hashtable[nameof(Todo.LabelColor).ToLower()];

            if(!Enum.TryParse<ColorName>(labelColor, out ColorName result)) {
                labelColor = ColorName.Transparent.ToString();
            }

            todo.LabelColorName = (labelColor == "" || labelColor == ColorName.Transparent.ToString()) ?
                 ColorName.Transparent : (ColorName)Enum.Parse(typeof(ColorName), labelColor, true);

            return todo;
        }

        public void changeDatabase(IDBConnectionStrings destDatabaseInfo) {
            connectionStringBuilder = new NpgsqlConnectionStringBuilder() {
                Host = destDatabaseInfo.HostName,
                Username = destDatabaseInfo.UserName,
                Database = "postgres",
                Password = destDatabaseInfo.PassWord,
                Port =destDatabaseInfo.PortNumber
            };

            if (tryConnect()) {
                CurrentServiceName = destDatabaseInfo.ServiceName;
                tryFirstConnectCommand();
            }
            else {
                TodoList = new List<Todo>();
            }
        }

        public bool tryConnect() {
            bool result = false;
            try {
                using (var con = DBConnection) {
                    con.Open();
                }
                result = true;
                Connected = true;
            }
            catch (TimeoutException) {
                Message = "接続を試行しましたがタイムアウトしました。データベースへの接続に失敗しました";
            }
            catch (SocketException) {
                Message = "接続を試行しましたが、接続先のサーバーが存在しません。";
            }
            catch (ArgumentException) {
                Message = "データベースへの接続に失敗しました。";
            }

            return result;
        }

        private void tryFirstConnectCommand() {
            loadTodoList();
            Message = $"データベースへの接続に成功。{CurrentServiceName} からTodoList をロードしました";

            if(DateTime.Now - Properties.Settings.Default.lastBackupDateTime > new TimeSpan(BackupDateInterval, 0, 0, 0)) {
                ExportAllCommand.Execute();
                Properties.Settings.Default.lastBackupDateTime = DateTime.Now;
                Properties.Settings.Default.Save();
            }
        }

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


        public DelegateCommand<Todo> CopyAndContinueCommand {
            #region
            get => copyAndContinueCommand ?? (copyAndContinueCommand = new DelegateCommand<Todo>((sourceTodo) => {
                Todo t = new Todo(sourceTodo) { Started = true};
                sourceTodo.CompleteCommand.Execute();
                insertTodo(t);
            }));
        }
        private DelegateCommand<Todo> copyAndContinueCommand;
        #endregion


        public DelegateCommand<Todo> ClearTextContentCommand {
            #region
            get => clearTextContentCommand ?? (clearTextContentCommand = new DelegateCommand<Todo>((sourceTodo) => {
                sourceTodo.TextContent = "";
                update(sourceTodo);
            }));
        }
        private DelegateCommand<Todo> clearTextContentCommand;
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

        /// <summary>
        /// データベースから全てのTodoを取り出してテキストファイルに出力します。
        /// </summary>
        public DelegateCommand ExportAllCommand {
            #region
            get => exportAllCommand ?? (exportAllCommand = new DelegateCommand(() => {
                var hashTable = select($"SELECT * FROM {TableName};", new List<NpgsqlParameter>());
                var todos = new List<Todo>();
                hashTable.ForEach((h) => todos.Add(toTodo(h)));

                using (var sw = new StreamWriter( @"backup.xml", false, new UTF8Encoding(false))) {
                    XmlSerializer serializer1 = new XmlSerializer(typeof(List<Todo>));
                    serializer1.Serialize(sw, todos);
                }

                var commentHashTable = select($"select * from {CommentTableName};", new List<NpgsqlParameter>());
                var comments = new List<Comment>();
                commentHashTable.ForEach((h) => {
                    comments.Add(new Comment() {
                        ID = (int)h[nameof(Comment.ID).ToLower()],
                        CreationDateTime = (DateTime)h[nameof(Comment.CreationDateTime).ToLower()],
                        TextContent = (String)h[nameof(Comment.TextContent).ToLower()]
                    });
                });

                using (var sw = new StreamWriter( @"backup-comment.xml", false, new UTF8Encoding(false))) {
                    new XmlSerializer(typeof(List<Comment>)).Serialize(sw, comments);
                }
            }));
        }
        private DelegateCommand exportAllCommand;
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
                long count = 0;
                try {
                    count = (long)(select( $"SELECT COUNT(*) FROM {TableName};", new List<NpgsqlParameter>())[0]["count"]);
                }catch(Exception e) {
                    if(e is TimeoutException || e is SocketException || e is ArgumentException) {
                        return 0;
                    }
                }

                return count;
            }
        }

        public int BackupDateInterval { get; set; } = 7;

        public SQLCommandOption SqlCommandOption { get; private set; }
    }
}
