using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WebTodoApp.Models {
    public class Todo : BindableBase {

        public Todo() {
        }

        /// <summary>
        /// 規定の Todo のプロパティをコピーして未完了、未作業状態の新しい Todo を作成します。
        /// </summary>
        /// <param name="existTodo"></param>
        public Todo(Todo existTodo) {
            Title = existTodo.Title;
            TextContent = existTodo.TextContent;
            Priority = existTodo.Priority;
            Duration = existTodo.Duration;
            CreationDate = DateTime.Now;
        }

        public int ID { get; set; } = -1;

        public DateTime CreationDate { get; set; } = new DateTime();
        public DateTime CompletionDate { get; set; } = new DateTime();
        public DateTime StartDateTime {
            get => startDateTime;
            set {

                // 初期値以外がセットされた場合は既に開始ボタンが一度押されているため false
                if (value.Ticks != 0) {
                    CanStart = false;
                }

                SetProperty(ref startDateTime, value);
            }
        }
        private DateTime startDateTime = new DateTime();

        public String Title { get => title; set => SetProperty(ref title, value); }
        private String title = "";

        public String TextContent { get => textContent; set => SetProperty(ref textContent, value); }
        private String textContent = "";

        // 優先順位は小さいほど高い
        public int Priority { get; set; } = 5;

        public String Tag { get; set; } = "";

        public bool Completed {
            get => completed;
            set {
                if (Started) {
                    TimeSpan ts = DateTime.Now - StartDateTime;
                    ActualDuration = (int)ts.TotalMinutes;
                    Started = false;
                    CanStart = false;
                }
                else if (value) {
                    CanStart = false;
                }

                SetProperty(ref completed, value);
            }
        }
        private bool completed;

        public int Duration { get; set; }

        public int ActualDuration { get; set; }

        public SolidColorBrush LabelColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorName.Transparent.ToString()));

        public ColorName LabelColorName { get; set; } = ColorName.Transparent;

        public bool Started {
            get => started;
            set {
                if (value && StartDateTime == new DateTime()) {
                    StartDateTime = DateTime.Now;
                    CanStart = false;
                }

                SetProperty(ref started, value);
            }
        }
        private bool started;

        public bool CanStart{
            get => canStart;
            set {
                SetProperty(ref canStart, value);
                RaisePropertyChanged(nameof(WorkingStatus));
            }
        }
        private bool canStart = true;

        /// <summary>
        /// この Todo オブジェクトがデータベースのデータを元に作られたものかどうかを示します。
        /// このプロパティが false である場合は、新規作成された Todo であることを示します。
        /// </summary>
        public bool existSource { get; set; }

        public String CreationDateShortString { get => CreationDate.ToString("MM/dd HH:mm"); }
        public String CompletionDateShortString { get =>
                (CompletionDate.Ticks == 0) ? "" : CompletionDate.ToString("MM/dd HH:mm");
        }

        /// <summary>
        /// このオブジェクトをパラメーターにしてデータベースに UPDATE をかける時、
        /// このプロパティに true がセットされていたらアップデートをしません。
        /// </summary>
        public bool updateStopping { get; set; }

        public DelegateCommand CompleteCommand {
            #region
            get => completeCommand ?? (completeCommand = new DelegateCommand(() => {
                if (!Completed) {
                    Completed = true;
                }

                CompletionDate = DateTime.Now;
                RaisePropertyChanged(nameof(CompletionDateShortString));
                RaisePropertyChanged(nameof(WorkingStatus));
            }));
        }
        private DelegateCommand completeCommand;
        #endregion

        /// <summary>
        /// 主に xaml でこのオブジェクトを取得する用のプロパティ。
        /// なくてもできるけど、あったほうが短くスマートに記述可能。
        /// </summary>
        public Todo Self => this;

        public String WorkingStatus {
            get {
                if(CompletionDate.Ticks != 0) {
                    return (ActualDuration == 0) ? " - " : $"{ActualDuration} min";
                }else if(StartDateTime.Ticks != 0) {
                    var elapsedDT = DateTime.Now - StartDateTime;
                    return $"{(int)elapsedDT.TotalMinutes} min";
                }else{
                    return "start";
                }
            }
        }

        /// <summary>
        /// WorkingStatus に表示されている、作業開始からの経過時間の変更をビューに通知します。
        /// </summary>
        public void updateElapsedTime() => RaisePropertyChanged(nameof(WorkingStatus));
        
        public void resetWorkingStatus() {
            Completed = false;
            Started = false;
            StartDateTime = new DateTime();
            ActualDuration = 0;
            CompletionDate = new DateTime();
            CanStart = true;
        }

    }
}
