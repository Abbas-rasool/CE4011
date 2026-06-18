using CommunityToolkit.Mvvm.ComponentModel;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// A support row. Maps to StructureInputData.SupportTable [NodeId, Rx, Ry, Rz]
/// (1 = restrained). References the node by object; the mapper resolves its id.
/// </summary>
public partial class SupportRowVm : ObservableObject
{
    [ObservableProperty] private NodeRowVm? node;

    [ObservableProperty] private bool restrainX;
    [ObservableProperty] private bool restrainY;
    [ObservableProperty] private bool restrainRz;
}
