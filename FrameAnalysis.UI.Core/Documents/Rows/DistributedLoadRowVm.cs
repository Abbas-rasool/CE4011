using CommunityToolkit.Mvvm.ComponentModel;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads;
using StructuralLoads;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// A uniformly distributed member load. Maps to StructureInputData.DistributedLoadTable
/// [ElementId, MagnitudePerLength, Direction, Nature]. Frame elements only.
/// </summary>
public partial class DistributedLoadRowVm : ObservableObject
{
    [ObservableProperty] private ElementRowVm? element;
    [ObservableProperty] private double magnitudePerLength;
    [ObservableProperty] private LoadDirection direction = LoadDirection.Y;

    /// <summary>
    /// Physical nature of this action (dead, live, wind, …). Drives which design-code load
    /// combinations the load participates in. Defaults to <see cref="eLoadNature.Dead"/>.
    /// </summary>
    [ObservableProperty] private eLoadNature nature = eLoadNature.Dead;
}
