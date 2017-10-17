using System.Windows;
using Caliburn.Micro;
using LonestarShowdown.Properties;

namespace LonestarShowdown.Shell
{
    public class AppBootstrapper : BootstrapperBase
    {
        private Window _mainWindow;

        public AppBootstrapper()
        {
            ConventionManager.ApplyValidation =
                (binding, viewModelType, property) => { binding.ValidatesOnExceptions = true; };
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<AppViewModel>();

            _mainWindow = Application.MainWindow;
            _mainWindow.Title = Resources.AppTitle;
            _mainWindow.MinHeight = 750;
            _mainWindow.MinWidth = 950;

            //mainWindow.Closing += MainWindowClosing;
        }
    }
}