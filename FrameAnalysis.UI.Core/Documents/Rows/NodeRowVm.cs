using CommunityToolkit.Mvvm.ComponentModel;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>A node row. Maps to StructureInputData.NodeTable [X, Y].</summary>
public partial class NodeRowVm : ObservableObject, IIdentifiedRow
{
    /// <summary>1-based id, maintained by <see cref="ProjectDocument"/> to match grid order.</summary>
    [ObservableProperty] private int id;

    [ObservableProperty] private double x;
    [ObservableProperty] private double y;

    /// <summary>Reserved for 3D. Hidden in the 2D grid; the mapper ignores it for now.</summary>
    [ObservableProperty] private double z;

    public override string ToString() => $"Node {Id}";
}
