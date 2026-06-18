namespace FrameAnalysis.UI.Core.Documents;

/// <summary>
/// Element type as shown in the UI. Values match StructureInputData.ElementTable
/// column 4 (0 = Frame, 1 = Truss) so the mapper can cast directly.
/// </summary>
public enum ElementKind
{
    Frame = 0,
    Truss = 1
}
