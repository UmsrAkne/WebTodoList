﻿namespace WebTodoApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using Prism.Commands;
    using Prism.Mvvm;

    [XmlInclude(typeof(Todo))]
    [Serializable]
    public class Todo : BindableBase
    {
        private string title = string.Empty;
        private string textContent = string.Empty;
        private bool completed;
        private SolidColorBrush labelColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorName.Transparent.ToString()));
        private DateTime startDateTime = new DateTime();
        private ColorName labelColorName = ColorName.Transparent;
        private int selectedColorIndex = 0;
        private bool started;
        private bool canStart = true;
        private DelegateCommand completeCommand;

        public Todo()
        {
            var colors = Enum.GetValues(typeof(ColorName));
            var colorList = new List<SolidColorBrush>();
            foreach (var colorName in colors)
            {
                string cn = colorName.ToString();
                colorList.Add(new SolidColorBrush((Color)ColorConverter.ConvertFromString(cn)));
            }

            SelectableLableColors = colorList;
        }

        /// <summary>
        /// 規定の Todo のプロパティをコピーして未完了、未作業状態の新しい Todo を作成します。
        /// </summary>
        /// <param name="existTodo"></param>
        public Todo(Todo existTodo) : this()
        {
            Title = existTodo.Title;
            TextContent = existTodo.TextContent;
            Priority = existTodo.Priority;
            Duration = existTodo.Duration;
            LabelColorName = existTodo.LabelColorName;
            CreationDate = DateTime.Now;
        }

        public int ID { get; set; } = -1;

        public DateTime CreationDate { get; set; } = new DateTime();

        public DateTime CompletionDate { get; set; } = new DateTime();

        public DateTime StartDateTime
        {
            get => startDateTime;
            set
            {
                // 初期値以外がセットされた場合は既に開始ボタンが一度押されているため false
                if (value.Ticks != 0)
                {
                    CanStart = false;
                }

                SetProperty(ref startDateTime, value);
            }
        }

        public string Title { get => title; set => SetProperty(ref title, value); }

        public string TextContent { get => textContent; set => SetProperty(ref textContent, value); }

        // 優先順位は小さいほど高い
        public int Priority { get; set; } = 5;

        public string Tag { get; set; } = string.Empty;

        public bool Completed
        {
            get => completed;
            set
            {
                if (Started)
                {
                    TimeSpan ts = DateTime.Now - StartDateTime;
                    ActualDuration = (int)ts.TotalMinutes;
                    Started = false;
                    CanStart = false;
                }
                else if (value)
                {
                    CanStart = false;
                }

                SetProperty(ref completed, value);
            }
        }

        public int Duration { get; set; }

        public int ActualDuration { get; set; }

        [XmlIgnore]
        public SolidColorBrush LabelColor
        {
            get => labelColor;
            private set => SetProperty(ref labelColor, value);
        }

        public ColorName LabelColorName
        {
            get => labelColorName;
            set
            {
                SetProperty(ref labelColorName, value);
                selectedColorIndex = (int)value;
                LabelColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(LabelColorName.ToString()));
            }
        }

        [XmlIgnore]
        public List<SolidColorBrush> SelectableLableColors { get; private set; }

        public int SelectedColorIndex
        {
            get => selectedColorIndex;
            set
            {
                LabelColorName = (ColorName)Enum.ToObject(typeof(ColorName), value);
                SetProperty(ref selectedColorIndex, value);
            }
        }

        public bool Started
        {
            get => started;
            set
            {
                if (value && StartDateTime == new DateTime())
                {
                    StartDateTime = DateTime.Now;
                    CanStart = false;
                }

                SetProperty(ref started, value);
            }
        }

        public bool CanStart
        {
            get => canStart;
            set
            {
                SetProperty(ref canStart, value);
                RaisePropertyChanged(nameof(WorkingStatus));
            }
        }

        /// <summary>
        /// この Todo オブジェクトがデータベースのデータを元に作られたものかどうかを示します。
        /// このプロパティが false である場合は、新規作成された Todo であることを示します。
        /// </summary>
        public bool ExistSource { get; set; }

        public string CreationDateShortString { get => CreationDate.ToString("MM/dd HH:mm"); }

        public string CompletionDateShortString
        {
            get => (CompletionDate.Ticks == 0) ? string.Empty : CompletionDate.ToString("MM/dd HH:mm");
        }

        /// <summary>
        /// このオブジェクトをパラメーターにしてデータベースに UPDATE をかける時、
        /// このプロパティに true がセットされていたらアップデートをしません。
        /// </summary>
        public bool UpdateStopping { get; set; }

        public DelegateCommand CompleteCommand
        {
            #region
            get => completeCommand ?? (completeCommand = new DelegateCommand(() =>
            {
                if (!Completed)
                {
                    Completed = true;
                }

                CompletionDate = DateTime.Now;
                RaisePropertyChanged(nameof(CompletionDateShortString));
                RaisePropertyChanged(nameof(WorkingStatus));
            }));
        }
        #endregion

        /// <summary>
        /// 主に xaml でこのオブジェクトを取得する用のプロパティ。
        /// なくてもできるけど、あったほうが短くスマートに記述可能。
        /// </summary>
        public Todo Self => this;

        public string WorkingStatus
        {
            get
            {
                if (CompletionDate.Ticks != 0)
                {
                    return (ActualDuration == 0) ? " - " : $"{ActualDuration} min";
                }
                else if (StartDateTime.Ticks != 0)
                {
                    var elapsedDT = DateTime.Now - StartDateTime;
                    return $"{(int)elapsedDT.TotalMinutes} min";
                }
                else
                {
                    return "start";
                }
            }
        }

        /// <summary>
        /// WorkingStatus に表示されている、作業開始からの経過時間の変更をビューに通知します。
        /// </summary>
        public void UpdateElapsedTime() => RaisePropertyChanged(nameof(WorkingStatus));

        public void ResetWorkingStatus()
        {
            Completed = false;
            Started = false;
            StartDateTime = new DateTime();
            ActualDuration = 0;
            CompletionDate = new DateTime();
            CanStart = true;
        }
    }
}
