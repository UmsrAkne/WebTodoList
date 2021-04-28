using WebTodoApp.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using WebTodoApp.ViewModels;

namespace WebTodoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<ConnectionDialog, ConnectionDialogViewModel>();
        }
    }
}
