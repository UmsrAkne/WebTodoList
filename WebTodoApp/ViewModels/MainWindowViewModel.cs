﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;
using WebTodoApp.Models;

namespace WebTodoApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Todo List";
        public string Title
        {
            get {
                if (DatabaseHelper != null && DatabaseHelper.WorkingTodos.Count == 1) {
                    foreach(Todo t in DatabaseHelper.WorkingTodos) {
                        return $"{t.WorkingStatus} {t.Title}";
                    }
                }

                return "Todo List";
            }

            set { SetProperty(ref _title, value); }
        }

        public DBHelper DatabaseHelper { get; set; }

        public Todo EnteringTodo {
            #region
            get => enteringTodo;
            private set => SetProperty(ref enteringTodo, value);
        }

        private Todo enteringTodo = new Todo();
        #endregion

        public MainWindowViewModel()
        {
            var dbserverName = (DBServerName)Enum.ToObject(typeof(DBServerName), Properties.Settings.Default.dbServerNumber);
            DatabaseHelper = new DBHelper("todo_table",dbserverName);
        }

        private DelegateCommand insertTodoCommand;
        public DelegateCommand InsertTodoCommand {
            get => insertTodoCommand ?? (insertTodoCommand = new DelegateCommand(() => {
                if(EnteringTodo.Title == "") {
                    return;
                }

                EnteringTodo.CreationDate = System.DateTime.Now;
                DatabaseHelper.insertTodo(EnteringTodo);
                EnteringTodo = new Todo();
            }));
        }

        public Visibility TextContentVisiblity { get; set; } = Visibility.Collapsed;
        public Visibility SideTextContentVisiblity { get; set; } = Visibility.Visible;

        public DelegateCommand ToggleTextContentVisibilityCommand {
            #region
            get => toggleTextContentVisibilityCommand ?? (toggleTextContentVisibilityCommand = new DelegateCommand(() => {
                if(TextContentVisiblity == Visibility.Visible) {
                    TextContentVisiblity = Visibility.Collapsed;
                    SideTextContentVisiblity = Visibility.Visible;
                }
                else {
                    TextContentVisiblity = Visibility.Visible;
                    SideTextContentVisiblity = Visibility.Collapsed;
                }

                RaisePropertyChanged(nameof(TextContentVisiblity));
                RaisePropertyChanged(nameof(SideTextContentVisiblity));

            }));
        }
        private DelegateCommand toggleTextContentVisibilityCommand;
        #endregion

        public DelegateCommand ExitCommand {
            #region
            get => exitCommand ?? (exitCommand = new DelegateCommand(() => {
                App.Current.Shutdown();
            }));
        }
        private DelegateCommand exitCommand;
        #endregion


        public DelegateCommand<UIElement> FocusCommand {
            #region
            get => focusCommand ?? (focusCommand = new DelegateCommand<UIElement>((focusableElement) => {
                focusableElement?.Focus();
            }));
        }
        private DelegateCommand<UIElement> focusCommand;
        #endregion

        public DelegateCommand<object> ChangeDatabaseServerCommand {
            #region
            get => changeDatabaseServerCommand ?? (changeDatabaseServerCommand = new DelegateCommand<object>((object dbinfo) => {
                DatabaseHelper.changeDatabase((DBServerName)dbinfo);
                Properties.Settings.Default.dbServerNumber = (int)dbinfo;
                Properties.Settings.Default.Save();
            }));
        }
        private DelegateCommand<object> changeDatabaseServerCommand;
        #endregion



    }
}
