using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace WebTodoApp.Models 
{
    public class KeyboardBehavior : Behavior<Window>
    {
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
        }

        //イベントハンドラ
        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e) {
        }
    }
}
