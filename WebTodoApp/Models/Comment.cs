namespace WebTodoApp.Models
{
    using Prism.Mvvm;
    using System;

    public class Comment : BindableBase
    {
        public int ID { get; set; }

        public DateTime CreationDateTime { get; set; }

        public string TextContent { get => textContent; set => SetProperty(ref textContent, value); }
        private string textContent = string.Empty;

        public string CreationDateShortString { get => CreationDateTime.ToString("MM/dd HH:mm"); }

    }
}
