using CommunityToolkit.Mvvm.ComponentModel;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// A thermal (temperature) member load. Maps to StructureInputData.TemperatureLoadTable
/// [ElementId, UniformTemperatureChange, TemperatureGradient, ThermalExpansionCoefficient,
/// MemberDepth]. Frame elements only.
/// </summary>
public partial class TemperatureLoadRowVm : ObservableObject
{
    [ObservableProperty] private ElementRowVm? element;
    [ObservableProperty] private double uniformTemperatureChange;
    [ObservableProperty] private double temperatureGradient;
    [ObservableProperty] private double thermalExpansionCoefficient;
    [ObservableProperty] private double memberDepth;
}
