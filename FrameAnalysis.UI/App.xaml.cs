using System.Windows;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysis.UI.Core.ViewModels;

namespace FrameAnalysis.UI
{
    /// <summary>
    /// Interaction logic for App.xaml. Composes the object graph at startup: the analysis
    /// service, the (seeded) document, the coordinator view-model, and the main window.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var viewModel = new MainViewModel(AnalysisService.CreateDefault(), SampleModels.Q4Frame());
            new MainWindow(viewModel).Show();
        }
    }
}
