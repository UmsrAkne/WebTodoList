using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    public class Todo : BindableBase {

        public int ID { get; set; } = -1;

        public DateTime CreationDate { get; set; } = new DateTime();
        public DateTime CompletionDate { get; set; } = new DateTime();
        public DateTime StartDateTime { get; set; } = new DateTime();

        public String Title { get => title; set => SetProperty(ref title, value); }
        private String title = "";

        public String TextContent { get => textContent; set => SetProperty(ref textContent, value); }
        private String textContent = "";

        // 優先順位は小さいほど高い
        public int Priority { get; set; } = 5;

        public String Tag { get; set; }

        public bool Completed {
            get => completed;
            set {
                if (Started) {
                    TimeSpan ts = DateTime.Now - StartDateTime;
                    ActualDuration = (int)ts.TotalMinutes;
                    Started = false;
                    CanStart = false;
                }
                else if(value){
                    CanStart = false;
                }

                SetProperty(ref completed, value);
            }
        }
        private bool completed;

        public int Duration { get; set; }

        public int ActualDuration { get; set; }

        public bool Started {
            get => started;
            set {
                if (value && StartDateTime == new DateTime()) {
                    StartDateTime = DateTime.Now;
                }

                SetProperty(ref started, value);
            }
        }
        private bool started;

        public bool CanStart{
            get => canStart;
            set {
                SetProperty(ref canStart, value);
            }
        }
        private bool canStart = true;

        /// <summary>
        /// この Todo オブジェクトがデータベースのデータを元に作られたものかどうかを示します。
        /// このプロパティが false である場合は、新規作成された Todo であることを示します。
        /// </summary>
        public bool existSource { get; set; }

        public String CreationDateShortString { get => CreationDate.ToString("yy/MM/dd/ HH:mm"); }
        public String CompletionDateShortString { get => CompletionDate.ToString("yy/MM/dd/ HH:mm"); }

        public DelegateCommand CompleteCommand {
            #region
            get => completeCommand ?? (completeCommand = new DelegateCommand(() => {
                CompletionDate = DateTime.Now;
            }));
        }
        private DelegateCommand completeCommand;
        #endregion
    }
}
