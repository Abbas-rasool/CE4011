using CommunityToolkit.Mvvm.ComponentModel;
using StructuralLoads;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// A joint load row. Maps to StructureInputData.LoadTable [NodeId, Fx, Fy, Mz, Nature]
/// in global coordinates.
/// </summary>
public partial class NodalLoadRowVm : ObservableObject
{
    [ObservableProperty] private NodeRowVm? node;

    [ObservableProperty] private double fx;
    [ObservableProperty] private double fy;
    [ObservableProperty] private double mz;

    /// <summary>
    /// Physical nature of this action (dead, live, wind, …). Drives which design-code load
    /// combinations the load participates in. Defaults to <see cref="eLoadNature.Dead"/>.
    /// </summary>
    [ObservableProperty] private eLoadNature nature = eLoadNature.Dead;
}
