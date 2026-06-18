using System;
using System.ComponentModel;
using System.Windows;
using FrameAnalysis.UI.Core.Reporting;
using FrameAnalysis.UI.Core.ViewModels;

namespace FrameAnalysis.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. Pure wiring: binds to the
    /// <see cref="MainViewModel"/> and bridges the WPF canvas to the renderer. All real
    /// behavior lives in the view-model and the (testable) UI.Core layer.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly Wpf2DCanvasRenderer _renderer;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;

            _renderer = new Wpf2DCanvasRenderer(ModelCanvas);
            _renderer.ElementPicked += (_, id) => _viewModel.SelectedElementId = id;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // The VM stays WPF-free: it asks for the popup, the window opens it here.
            _viewModel.MemberResultRequested += (stations, label) =>
                new MemberResultWindow(stations, label) { Owner = this }.Show();

            _viewModel.DesignReportRequested += OnDesignReportRequested;

            Loaded += (_, _) =>
            {
                _renderer.Render(_viewModel.CurrentScene);
                _renderer.ZoomToFit();
            };
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentScene))
                _renderer.Render(_viewModel.CurrentScene);
            else if (e.PropertyName == nameof(MainViewModel.SelectedElementId))
                _renderer.SetSelectedElement(_viewModel.SelectedElementId);
        }

        private void ZoomExtents_Click(object sender, RoutedEventArgs e) => _renderer.ZoomToFit();

        /// <summary>Prompts for a path, generates the design-report PDF, and opens it.
        /// The VM raised this; all the WPF-specific work (dialog, shell open) lives here.</summary>
        private void OnDesignReportRequested()
        {
            var outcome = _viewModel.LastDesignOutcome;
            if (outcome is null)
                return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Design Report",
                Filter = "PDF document (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = $"{SanitizeFileName(_viewModel.Document.ProjectName)}_DesignReport.pdf"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                DesignReportGenerator.Save(dialog.FileName, _viewModel.Document, outcome);
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not generate the report:\n{ex.Message}",
                    "Export failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Project";
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
