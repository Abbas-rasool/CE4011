using System.ComponentModel;
using System.Windows;
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
    }
}
