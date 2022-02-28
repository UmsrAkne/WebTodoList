using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace WebTodoApp.Models
{
    public class TextInputLimitBehavior : Behavior<TextBox>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            ((TextBox)AssociatedObject).PreviewTextInput += previewTextInputEventHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            ((TextBox)AssociatedObject).PreviewTextInput -= previewTextInputEventHandler;
        }

        //イベントハンドラ
        private void previewTextInputEventHandler(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out int result))
            {
                e.Handled = true;
            }
        }
    }

}
