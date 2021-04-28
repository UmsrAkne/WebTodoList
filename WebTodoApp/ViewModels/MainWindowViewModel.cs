using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using WebTodoApp.Models;
using WebTodoApp.Views;

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

        private IDialogService DialogService { get; set; } 

        public MainWindowViewModel(IDialogService dialogService)
        {
            DialogService = dialogService;
            DatabaseHelper = new DBHelper("todo_table",new AnyDBConnectionStrings("certification"));
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

        public DelegateCommand ShowConnectionDialogCommand {
            #region
            get => showConnectionDialogCommand ?? (showConnectionDialogCommand = new DelegateCommand(() => {
                var param = new DialogParameters();
                DialogService.ShowDialog(nameof(ConnectionDialog), param, (IDialogResult result) => {
                    if(result.Result == ButtonResult.Yes) {
                        var dbConnectionInfo = result.Parameters.GetValue<IDBConnectionStrings>(nameof(AnyDBConnectionStrings));
                        DatabaseHelper.changeDatabase(dbConnectionInfo);
                    }
                });
            }));
        }
        private DelegateCommand showConnectionDialogCommand;
        #endregion




    }
}
