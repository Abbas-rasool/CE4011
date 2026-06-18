using System.Windows;

namespace FrameAnalysis.UI.Converters
{
    /// <summary>
    /// Carries a DataContext into places that are not part of the visual tree — notably
    /// <see cref="System.Windows.Controls.DataGridColumn"/>, whose <c>Visibility</c> cannot
    /// otherwise bind to the grid's DataContext. Declare it in the element's resources with
    /// <c>Data="{Binding}"</c>; Freezables inherit the DataContext, so the proxy exposes it via
    /// <see cref="Data"/> for bindings that supply <c>Source={StaticResource ...}</c>.
    /// </summary>
    public sealed class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
