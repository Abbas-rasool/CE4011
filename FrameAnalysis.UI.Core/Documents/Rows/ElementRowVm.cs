using CommunityToolkit.Mvvm.ComponentModel;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// An element row. Maps to StructureInputData.ElementTable
/// [StartNodeId, EndNodeId, MaterialId, SectionId, ElementType, MomentRelease].
/// References other rows by object (not raw id) so grid reorder/insert/delete stays safe;
/// the mapper resolves these to 1-based ids at export.
/// </summary>
public partial class ElementRowVm : ObservableObject, IIdentifiedRow
{
    [ObservableProperty] private int id;

    [ObservableProperty] private NodeRowVm? startNode;
    [ObservableProperty] private NodeRowVm? endNode;
    [ObservableProperty] private MaterialRowVm? material;
    [ObservableProperty] private SectionRowVm? section;

    [ObservableProperty] private ElementKind kind = ElementKind.Frame;

    /// <summary>Frame-only end release; ignored for truss members.</summary>
    [ObservableProperty] private MomentRelease release = MomentRelease.None;

    public override string ToString() => $"Member {Id}";
}
