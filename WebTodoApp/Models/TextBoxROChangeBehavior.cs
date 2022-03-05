namespace WebTodoApp.Models
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    public class TextBoxROChangeBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            ((TextBox)AssociatedObject).MouseDoubleClick += MouseDoubleClickeHandler;
            ((TextBox)AssociatedObject).LostKeyboardFocus += LostKeyboardFocusHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            ((TextBox)AssociatedObject).MouseDoubleClick += MouseDoubleClickeHandler;
            ((TextBox)AssociatedObject).LostKeyboardFocus += LostKeyboardFocusHandler;
        }

        private void MouseDoubleClickeHandler(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).IsReadOnly = false;
            ((TextBox)sender).SelectAll();
        }

        private void LostKeyboardFocusHandler(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).IsReadOnly = true;
        }
    }
}
