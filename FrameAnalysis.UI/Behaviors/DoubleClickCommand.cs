using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrameAnalysis.UI.Behaviors
{
    /// <summary>
    /// Attaches an <see cref="ICommand"/> to a control's double-click. Unlike a
    /// <c>MouseBinding</c> in <c>InputBindings</c>, an attached property participates in
    /// DataContext inheritance, so the command binding resolves correctly inside a
    /// DataTemplate (which is why the Members list uses this rather than an InputBinding).
    /// </summary>
    public static class DoubleClickCommand
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(DoubleClickCommand),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);
        public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Control control)
                return;

            control.MouseDoubleClick -= OnMouseDoubleClick;
            if (e.NewValue is ICommand)
                control.MouseDoubleClick += OnMouseDoubleClick;
        }

        private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ICommand command = GetCommand((DependencyObject)sender);
            if (command is not null && command.CanExecute(null))
                command.Execute(null);
        }
    }
}
