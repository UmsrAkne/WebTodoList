﻿namespace WebTodoApp.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Media;
    using System.Net.Sockets;
    using System.Text;
    using System.Timers;
    using System.Windows.Controls;
    using System.Xml.Serialization;
    using Npgsql;
    using Prism.Commands;
    using Prism.Mvvm;

    public class DBHelper : BindableBase
    {
        private NpgsqlConnectionStringBuilder connectionStringBuilder;
        private List<Todo> todoList = new List<Todo>();
        private Timer timer = new Timer(10000);
        private SoundPlayer soundPlayer = new SoundPlayer(@"C:\Windows\Media\Windows Notify Calendar.wav");
        private DelegateCommand<object> updateCommand;
        private DelegateCommand<Todo> copyTodoCommand;
        private DelegateCommand loadCommand;
        private DelegateCommand<Todo> copyTodoWithoutTextCommand;
        private DelegateCommand<Todo> copyAndContinueCommand;
        private DelegateCommand<Todo> clearTextContentCommand;
        private DelegateCommand<Todo> resetTodoWorkingStatusCommand;
        private DelegateCommand exportAllCommand;
        private string message = string.Empty;

        public DBHelper(string tableName, IDBConnectionStrings dbconnectionStrings) : this(tableName)
        {
            ChangeDatabase(dbconnectionStrings);
        }

        private DBHelper(string tableName)
        {
            TableName = tableName;
            /// createTable();

            SqlCommandOption = new SQLCommandOption();
            SqlCommandOption.Limit = 100;
            SqlCommandOption.TableName = TableName;
            SqlCommandOption.OrderByColumns.Add(
                new SQLCommandOption.SQLCommandColumnOption()
                {
                    Name = nameof(Todo.CreationDate),
                    DESC = true
                });

            timer.Elapsed += (sender, e) =>
            {
                foreach (Todo t in WorkingTodos)
                {
                    t.UpdateElapsedTime();

                    if ((int)t.Duration == (int)(DateTime.Now - t.StartDateTime).TotalMinutes && !t.Completed)
                    {
                        soundPlayer.Play();
                    }
                }
            };

            timer.Start();
        }

        public List<Todo> TodoList { get => todoList; private set => SetProperty(ref todoList, value); }

        public HashSet<Todo> WorkingTodos { get; set; } = new HashSet<Todo>();

        public bool Connected { get; set; } = false;

        public DelegateCommand<object> UpdateCommand
        {
            #region
            get => updateCommand ?? (updateCommand = new DelegateCommand<object>((object l) =>
            {
                Todo todo = ((ListViewItem)l).Content as Todo;
                if (todo != null)
                {
                    Update(todo);
                    if (todo.Started)
                    {
                        WorkingTodos.Add(todo);
                    }
                }
            }));
        }
        #endregion

        public string TableName { get; private set; }

        public string CommentTableName { get; private set; } = "comments";

        public DelegateCommand LoadCommand
        {
            #region
            get => loadCommand ?? (loadCommand = new DelegateCommand(() =>
            {
                LoadTodoList();
                Message = "TodoList をリロードしました。";
            }));
        }
        #endregion

        public DelegateCommand<Todo> CopyTodoCommand
        {
            #region
            get => copyTodoCommand ?? (copyTodoCommand = new DelegateCommand<Todo>((sourceTodo) =>
            {
                InsertTodo(new Todo(sourceTodo));
            }));
        }
        #endregion

        public DelegateCommand<Todo> CopyTodoWithoutTextCommand
        {
            #region
            get => copyTodoWithoutTextCommand ?? (copyTodoWithoutTextCommand = new DelegateCommand<Todo>((sourceTodo) =>
            {
                Todo t = new Todo(sourceTodo) { TextContent = string.Empty };
                InsertTodo(t);
            }));
        }
        #endregion

        public DelegateCommand<Todo> CopyAndContinueCommand
        {
            #region
            get => copyAndContinueCommand ?? (copyAndContinueCommand = new DelegateCommand<Todo>((sourceTodo) =>
            {
                Todo t = new Todo(sourceTodo) { Started = true };
                sourceTodo.CompleteCommand.Execute();
                InsertTodo(t);
            }));
        }
        #endregion

        public DelegateCommand<Todo> ClearTextContentCommand
        {
            #region
            get => clearTextContentCommand ?? (clearTextContentCommand = new DelegateCommand<Todo>((sourceTodo) =>
            {
                sourceTodo.TextContent = string.Empty;
                Update(sourceTodo);
            }));
        }
        #endregion

        public DelegateCommand<Todo> ResetTodoWorkingStatusCommand
        {
            #region
            get => resetTodoWorkingStatusCommand ?? (resetTodoWorkingStatusCommand = new DelegateCommand<Todo>((targetTodo) =>
            {
                targetTodo.ResetWorkingStatus();
                Update(targetTodo);
            }));
        }
        #endregion

        /// <summary>
        /// データベースから全てのTodoを取り出してテキストファイルに出力します。
        /// </summary>
        public DelegateCommand ExportAllCommand
        {
            #region
            get => exportAllCommand ?? (exportAllCommand = new DelegateCommand(() =>
            {
                var hashTable = Select($"SELECT * FROM {TableName};", new List<NpgsqlParameter>());
                var todos = new List<Todo>();
                hashTable.ForEach((h) => todos.Add(ToTodo(h)));

                using (var sw = new StreamWriter(@"backup.xml", false, new UTF8Encoding(false)))
                {
                    XmlSerializer serializer1 = new XmlSerializer(typeof(List<Todo>));
                    serializer1.Serialize(sw, todos);
                }

                var commentHashTable = Select($"select * from {CommentTableName};", new List<NpgsqlParameter>());
                var comments = new List<Comment>();
                commentHashTable.ForEach((h) =>
                {
                    comments.Add(new Comment()
                    {
                        ID = (int)h[nameof(Comment.ID).ToLower()],
                        CreationDateTime = (DateTime)h[nameof(Comment.CreationDateTime).ToLower()],
                        TextContent = (string)h[nameof(Comment.TextContent).ToLower()]
                    });
                });

                using (var sw = new StreamWriter(@"backup-comment.xml", false, new UTF8Encoding(false)))
                {
                    new XmlSerializer(typeof(List<Comment>)).Serialize(sw, comments);
                }
            }));
        }
        #endregion

        public string Message
        {
            get => message;
            set
            {
                value = $"{DateTime.Now} " + value;
                SetProperty(ref message, value);
            }
        }

        public long TodoCount
        {
            get
            {
                long count = 0;
                try
                {
                    count = (long)Select($"SELECT COUNT(*) FROM {TableName};", new List<NpgsqlParameter>())[0]["count"];
                }
                catch (Exception e)
                {
                    if (e is TimeoutException || e is SocketException || e is ArgumentException)
                    {
                        return 0;
                    }
                }

                return count;
            }
        }

        public int BackupDateInterval { get; set; } = 7;

        public SQLCommandOption SqlCommandOption { get; private set; }

        private NpgsqlConnection DBConnection
        {
            get => new NpgsqlConnection(connectionStringBuilder.ToString());
        }

        private string CurrentServiceName { get; set; }

        public void InsertTodo(Todo todo)
        {
            var count = Select($"SELECT COUNT (*) FROM {TableName};", new List<NpgsqlParameter>())[0];

            int maxID = 0;

            if ((long)count["count"] > 0)
            {
                var maxIDRow = Select($"SELECT MAX ({nameof(Todo.ID)}) FROM {TableName};", new List<NpgsqlParameter>())[0];
                maxID = (int)maxIDRow["max"] + 1;
            }

            var ps = new List<NpgsqlParameter>();

            // ユーザーの入力が入る余地がある部分はパラメーターによる入力を行う。
            // もともと固定になっている部分はそうでなくてもOKなはず
            ps.Add(new NpgsqlParameter(nameof(Todo.Title), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Title });
            ps.Add(new NpgsqlParameter(nameof(Todo.TextContent), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.TextContent });
            ps.Add(new NpgsqlParameter(nameof(Todo.Priority), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Priority });
            ps.Add(new NpgsqlParameter(nameof(Todo.Duration), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Duration });
            ps.Add(new NpgsqlParameter(nameof(Todo.Tag), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Tag });

            StringBuilder query = new StringBuilder();

            query.Append($"INSERT INTO {TableName} ( ");
            query.Append($"{nameof(Todo.ID)}, ");
            query.Append($"{nameof(Todo.Completed)}, ");
            query.Append($"{nameof(Todo.Title)}, ");
            query.Append($"{nameof(Todo.TextContent)},");
            query.Append($"{nameof(Todo.CreationDate)},");
            query.Append($"{nameof(Todo.CompletionDate)},");
            query.Append($"{nameof(Todo.StartDateTime)},");
            query.Append($"{nameof(Todo.Priority)},");
            query.Append($"{nameof(Todo.Duration)},");
            query.Append($"{nameof(Todo.LabelColor)},");
            query.Append($"{nameof(Todo.Tag)} ) ");
            query.Append($"VALUES (");
            query.Append($"{maxID},");
            query.Append($"{todo.Completed},");
            query.Append($":{nameof(todo.Title)},");
            query.Append($":{nameof(todo.TextContent)},");
            query.Append($"'{todo.CreationDate}',");
            query.Append($"'{todo.CompletionDate}',");
            query.Append($"'{todo.StartDateTime}',");
            query.Append($":{nameof(todo.Priority)},");
            query.Append($":{nameof(todo.Duration)},");
            query.Append($"'{todo.LabelColorName}',");
            query.Append($":{nameof(todo.Tag)}");
            query.Append(");");

            ExecuteNonQuery(query.ToString(), ps);

            RaisePropertyChanged(nameof(TodoCount));
            LoadTodoList();
        }

        public void Update(Todo todo)
        {
            System.Diagnostics.Debug.WriteLine(todo);
            if (todo.ID < 0 || !todo.ExistSource)
            {
                // 上記を満たす場合、更新すべきTodoのソース（行）が存在しない
                throw new ArgumentException("更新すべき行を特定できないTodoが指定されました。");
            }

            if (todo.UpdateStopping)
            {
                return;
            }

            var ps = new List<NpgsqlParameter>();

            ps.Add(new NpgsqlParameter(nameof(Todo.Title), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Title });
            ps.Add(new NpgsqlParameter(nameof(Todo.TextContent), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.TextContent });
            ps.Add(new NpgsqlParameter(nameof(Todo.Priority), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Priority });
            ps.Add(new NpgsqlParameter(nameof(Todo.Duration), NpgsqlTypes.NpgsqlDbType.Integer) { Value = todo.Duration });
            ps.Add(new NpgsqlParameter(nameof(Todo.Tag), NpgsqlTypes.NpgsqlDbType.Text) { Value = todo.Tag });

            StringBuilder query = new StringBuilder();
            query.Append($"update {TableName} SET ");
            query.Append($"{nameof(Todo.Completed)} = {todo.Completed}, ");
            query.Append($"{nameof(Todo.Title)} = :{nameof(todo.Title)}, ");
            query.Append($"{nameof(Todo.TextContent)} = :{nameof(todo.TextContent)}, ");
            query.Append($"{nameof(Todo.CreationDate)} = '{todo.CreationDate}', ");
            query.Append($"{nameof(Todo.CompletionDate)} = '{todo.CompletionDate}', ");
            query.Append($"{nameof(Todo.StartDateTime)} = '{todo.StartDateTime}', ");
            query.Append($"{nameof(Todo.Priority)} = :{nameof(todo.Priority)}, ");
            query.Append($"{nameof(Todo.Duration)} = :{nameof(todo.Duration)}, ");
            query.Append($"{nameof(Todo.Tag)} = :{nameof(todo.Tag)}, ");
            query.Append($"{nameof(Todo.LabelColor)} = '{todo.LabelColorName}' ");
            query.Append($"WHERE id = {todo.ID};");

            ExecuteNonQuery(query.ToString(), ps);
        }

        public void ChangeDatabase(IDBConnectionStrings destDatabaseInfo)
        {
            connectionStringBuilder = new NpgsqlConnectionStringBuilder()
            {
                Host = destDatabaseInfo.HostName,
                Username = destDatabaseInfo.UserName,
                Database = "postgres",
                Password = destDatabaseInfo.PassWord,
                Port = destDatabaseInfo.PortNumber
            };

            if (TryConnect())
            {
                CurrentServiceName = destDatabaseInfo.ServiceName;
                TryFirstConnectCommand();
            }
            else
            {
                TodoList = new List<Todo>();
            }
        }

        public bool TryConnect()
        {
            bool result = false;
            try
            {
                using (var con = DBConnection)
                {
                    con.Open();
                }

                result = true;
                Connected = true;
            }
            catch (TimeoutException)
            {
                Message = "接続を試行しましたがタイムアウトしました。データベースへの接続に失敗しました";
            }
            catch (SocketException)
            {
                Message = "接続を試行しましたが、接続先のサーバーが存在しません。";
            }
            catch (ArgumentException)
            {
                Message = "データベースへの接続に失敗しました。";
            }

            return result;
        }

        private void TryFirstConnectCommand()
        {
            LoadTodoList();
            Message = $"データベースへの接続に成功。{CurrentServiceName} からTodoList をロードしました";

            if (DateTime.Now - Properties.Settings.Default.lastBackupDateTime > new TimeSpan(BackupDateInterval, 0, 0, 0))
            {
                ExportAllCommand.Execute();
                Properties.Settings.Default.lastBackupDateTime = DateTime.Now;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// select で取得した dataReader から取り出した値を Hashtable に詰め、それをまとめたリストを取得します。
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        private List<Hashtable> Select(string commandText, List<NpgsqlParameter> parameters)
        {
            var startTime = DateTime.Now;

            using (var con = DBConnection)
            {
                List<Hashtable> resultList = new List<Hashtable>();
                con.Open();
                var command = new NpgsqlCommand(commandText, con);
                parameters.ForEach(p => command.Parameters.Add(p));
                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    var hashtable = new Hashtable();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        hashtable[dataReader.GetName(i)] = dataReader.GetValue(i);
                    }

                    resultList.Add(hashtable);
                }

                TimeSpan processDuration = DateTime.Now - startTime;
                File.AppendAllText(@"sqllog.txt", $"{DateTime.Now} {processDuration}\t{commandText}{Environment.NewLine}");

                return resultList;
            }
        }

        private void ExecuteNonQuery(string commandText, List<NpgsqlParameter> commandParams)
        {
            var startTime = DateTime.Now;

            using (var con = DBConnection)
            {
                con.Open();
                var command = new NpgsqlCommand(commandText, con);
                commandParams.ForEach((param) => { command.Parameters.Add(param); });
                command.ExecuteNonQuery();
            }

            TimeSpan processDuration = DateTime.Now - startTime;
            File.AppendAllText(@"sqllog.txt", $"{DateTime.Now} {processDuration}\t{commandText}{Environment.NewLine}");
        }

        /// <summary>
        /// if not exists を含むテーブル生成用の sql文 を実行します。
        /// </summary>
        private void CreateTable()
        {
            StringBuilder query = new StringBuilder();
            query.Append($"CREATE TABLE IF NOT EXISTS {TableName} (");
            query.Append($"id INTEGER PRIMARY KEY, ");
            query.Append($"{nameof(Todo.Completed)} BOOLEAN NOT NULL, ");
            query.Append($"{nameof(Todo.Title)} TEXT NOT NULL, ");
            query.Append($"{nameof(Todo.TextContent)} TEXT NOT NULL, ");
            query.Append($"{nameof(Todo.CreationDate)} TIMESTAMP NOT NULL, ");
            query.Append($"{nameof(Todo.CompletionDate)} TIMESTAMP NOT NULL, ");
            query.Append($"{nameof(Todo.Priority)} INTEGER NOT NULL, ");
            query.Append($"{nameof(Todo.Duration)} INTEGER DEFAULT 0 NOT NULL, ");
            query.Append($"{nameof(Todo.StartDateTime)} TIMESTAMP NOT NULL DEFAULT '0001/01/01 0:00:00', ");
            query.Append($"{nameof(Todo.LabelColor)} TEXT NOT NULL, ");
            query.Append($"{nameof(Todo.Tag)} TEXT NOT NULL ");
            query.Append(");");

            ExecuteNonQuery(query.ToString(), new List<NpgsqlParameter>());

            StringBuilder commentTableQuery = new StringBuilder();
            commentTableQuery.Append($"CREATE TABLE IF NOT EXISTS {CommentTableName} (");
            commentTableQuery.Append($"{nameof(Comment.ID)} INTEGER PRIMARY KEY, ");
            commentTableQuery.Append($"{nameof(Comment.CreationDateTime)} TIMESTAMP NOT NULL,");
            commentTableQuery.Append($"{nameof(Comment.TextContent)} TEXT NOT NULL ");
            commentTableQuery.Append($");");

            ExecuteNonQuery(commentTableQuery.ToString(), new List<NpgsqlParameter>());
        }

        private void LoadTodoList()
        {
            var rows = Select(SqlCommandOption.BuildSQL(), SqlCommandOption.SqlParams);
            var list = new List<Todo>();

            WorkingTodos.Clear();

            rows.ForEach((Hashtable row) =>
            {
                var t = ToTodo(row);

                if (t.Started)
                {
                    WorkingTodos.Add(t);
                }

                list.Add(t);
            });

            TodoList = list;
        }

        private Todo ToTodo(Hashtable hashtable)
        {
            Todo todo = new Todo();

            todo.ExistSource = true;

            todo.ID = (int)hashtable[nameof(Todo.ID).ToLower()];
            todo.Completed = (bool)hashtable[nameof(Todo.Completed).ToLower()];
            todo.Title = (string)hashtable[nameof(Todo.Title).ToLower()];
            todo.TextContent = (string)hashtable[nameof(Todo.TextContent).ToLower()];

            todo.CreationDate = (DateTime)hashtable[nameof(Todo.CreationDate).ToLower()];
            todo.CompletionDate = (DateTime)hashtable[nameof(Todo.CompletionDate).ToLower()];
            todo.StartDateTime = (DateTime)hashtable[nameof(Todo.StartDateTime).ToLower()];

            if (todo.CompletionDate.Ticks != 0)
            {
                // 既に完了している状態
                todo.Started = false;
                todo.CanStart = false;
            }
            else
            {
                // todo 未完了の状態
                todo.Started = todo.StartDateTime.Ticks != 0;
                todo.CanStart = todo.CompletionDate.Ticks == 0 && todo.StartDateTime.Ticks == 0;
            }

            if (todo.CompletionDate.Ticks != 0 && todo.StartDateTime.Ticks != 0)
            {
                todo.ActualDuration = (int)(todo.CompletionDate - todo.StartDateTime).TotalMinutes;
            }

            todo.Priority = (int)hashtable[nameof(Todo.Priority).ToLower()];
            todo.Duration = (int)hashtable[nameof(Todo.Duration).ToLower()];
            todo.Tag = (string)hashtable[nameof(Todo.Tag).ToLower()];

            string labelColor = (string)hashtable[nameof(Todo.LabelColor).ToLower()];

            if (!Enum.TryParse<ColorName>(labelColor, out ColorName result))
            {
                labelColor = ColorName.Transparent.ToString();
            }

            todo.LabelColorName = (labelColor == string.Empty || labelColor == ColorName.Transparent.ToString()) ?
                 ColorName.Transparent : (ColorName)Enum.Parse(typeof(ColorName), labelColor, true);

            return todo;
        }
    }
}
