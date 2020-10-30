using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTodoApp.Models {
    public class Todo: BindableBase {

        public int ID { get; set; } = -1;

        public DateTime CreationDate { get; set; } = new DateTime();
        public DateTime CompletionDate { get; set; } = new DateTime();

        public String Title { get => title; set => SetProperty(ref title, value); }
        private String title = "";

        public String TextContent { get => textContent; set => SetProperty(ref textContent, value); }
        private String textContent = "";

        // 優先順位は小さいほど高い
        public int Priority {get; set;} = 5;

        public String Tag { get; set; }

        public bool Completed { get; set; }

        public int Duration { get; set; }

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
