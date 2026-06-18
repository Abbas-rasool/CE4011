using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FrameAnalysis.UI.Core.Documents;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> that also raises <see cref="Changed"/> when
/// any contained row's properties change — not only on add/remove/move/clear.
///
/// Plain ObservableCollection notifies on structural edits but is silent when a user
/// edits a cell of an existing row. By tracking each item's <see cref="INotifyPropertyChanged"/>,
/// this type gives a single subscriber (the renderer / scene pipeline) one signal for
/// every kind of edit. Handlers are attached on insert and detached on remove/replace/clear,
/// so there are no leaks.
/// </summary>
public sealed class ObservableRowCollection<T> : ObservableCollection<T>
    where T : INotifyPropertyChanged
{
    /// <summary>Raised on any structural change or any contained row's property change.</summary>
    public event EventHandler? Changed;

    protected override void InsertItem(int index, T item)
    {
        item.PropertyChanged += OnItemPropertyChanged;
        base.InsertItem(index, item);
        RaiseChanged();
    }

    protected override void RemoveItem(int index)
    {
        this[index].PropertyChanged -= OnItemPropertyChanged;
        base.RemoveItem(index);
        RaiseChanged();
    }

    protected override void SetItem(int index, T item)
    {
        this[index].PropertyChanged -= OnItemPropertyChanged;
        item.PropertyChanged += OnItemPropertyChanged;
        base.SetItem(index, item);
        RaiseChanged();
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        base.MoveItem(oldIndex, newIndex);
        RaiseChanged();
    }

    protected override void ClearItems()
    {
        foreach (T item in this)
            item.PropertyChanged -= OnItemPropertyChanged;
        base.ClearItems();
        RaiseChanged();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => RaiseChanged();

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
