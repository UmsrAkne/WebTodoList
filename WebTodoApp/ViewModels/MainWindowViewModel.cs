﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Controls;
using WebTodoApp.Models;

namespace WebTodoApp.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
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

        public Comment EnteringComment {
            #region
            get => enteringComment;
            set => SetProperty(ref enteringComment, value);
        }
        private Comment enteringComment = new Comment();
        #endregion

        public MainWindowViewModel()
        {
            DatabaseHelper = new DBHelper("todo_table");
        }

        private DelegateCommand insertTodoCommand;
        public DelegateCommand InsertTodoCommand {
            get => insertTodoCommand ?? (insertTodoCommand = new DelegateCommand(() => {
                if(EnteringTodo.Title == "" && EnteringTodo.TextContent == "") {
                    return;
                }

                EnteringTodo.CreationDate = System.DateTime.Now;
                DatabaseHelper.insertTodo(EnteringTodo);
                EnteringTodo = new Todo();
            }));
        }


        public DelegateCommand InsertCommentCommand {
            #region
            get => insertCommentCommand ?? (insertCommentCommand = new DelegateCommand(() => {
                if(EnteringComment.TextContent != "") {
                    EnteringComment.CreationDateTime = DateTime.Now;
                    DatabaseHelper.insertComment(EnteringComment);
                    EnteringComment = new Comment();
                }
            }));
        }
        private DelegateCommand insertCommentCommand;
        #endregion


        /// <summary>
        /// パラメーターに受け取った TextBox を編集可能状態にします。
        /// </summary>
        public DelegateCommand<TextBox> ToWritableCommand {
            #region
            get => toWritableCommand ?? (toWritableCommand = new DelegateCommand<TextBox>( (TextBox textBox) => {
                textBox.IsReadOnly = false;
                textBox.SelectAll();
            }));
        }
        private DelegateCommand<TextBox> toWritableCommand;
        #endregion

        public DelegateCommand<TextBox> ToReadOnlyCommand {
            #region
            get => toReadOnlyCommand ?? (toReadOnlyCommand = new DelegateCommand<TextBox>((TextBox textBox) => {
                textBox.IsReadOnly = true;
            }));
        }
        private DelegateCommand<TextBox> toReadOnlyCommand;
        #endregion


        public DelegateCommand ExitCommand {
            #region
            get => exitCommand ?? (exitCommand = new DelegateCommand(() => {
                App.Current.Shutdown();
            }));
        }
        private DelegateCommand exitCommand;
        #endregion


    }
}
