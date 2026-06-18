using CommunityToolkit.Mvvm.ComponentModel;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// A support settlement (prescribed displacement). Maps to StructureInputData.SettlementTable
/// [NodeId, dUx, dUy, dRz]. Only applied at a DOF that is restrained in the support table.
/// </summary>
public partial class SettlementRowVm : ObservableObject
{
    [ObservableProperty] private NodeRowVm? node;
    [ObservableProperty] private double deltaUx;
    [ObservableProperty] private double deltaUy;
    [ObservableProperty] private double deltaRz;
}
