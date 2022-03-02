namespace WebTodoApp
{
    using System.Windows;
    using Prism.Ioc;
    using WebTodoApp.Views;
    using WebTodoApp.ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ConnectionDialog, ConnectionDialogViewModel>();
        }
    }
}
