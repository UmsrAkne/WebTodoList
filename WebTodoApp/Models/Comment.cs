namespace WebTodoApp.Models
{
    using System;
    using Prism.Mvvm;

    public class Comment : BindableBase
    {
        private string textContent = string.Empty;

        public int ID { get; set; }

        public DateTime CreationDateTime { get; set; }

        public string TextContent { get => textContent; set => SetProperty(ref textContent, value); }

        public string CreationDateShortString { get => CreationDateTime.ToString("MM/dd HH:mm"); }
    }
}
