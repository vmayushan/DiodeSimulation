using System.Windows;
using SimulationTool;
using SimulationTool.Dialogs;
using SimulationTool.View;
using SimulationTool.ViewModel;

namespace SimulationTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Create the ViewModel and expose it using the View's DataContext
            var view = new MainWindow() { DataContext = new MainViewModel(DialogCoordinator.Instance) };
            view.Show();
        }
    }
}
