using CommunityToolkit.Mvvm.ComponentModel;
using StructuralLoads;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Documents;

/// <summary>
/// Project-wide design settings (the Design tab's "Project Details"). The design code lives
/// here, not in global app setup — analysis stays code-agnostic. The environment field is
/// code-aware: <see cref="ServiceClass"/> applies to EC5/TR, <see cref="MoistureCondition"/>
/// to US.
/// </summary>
public partial class DesignSettings : ObservableObject
{
    /// <summary>Active design code for the whole project (US / EC5 / TR).</summary>
    [ObservableProperty] private eTimberCode code = eTimberCode.EC5;

    /// <summary>US (ASCE 7) combination format — LRFD or ASD. Ignored by EC5 / TR.</summary>
    [ObservableProperty] private eLoadCombinationType usDesignMethod = eLoadCombinationType.ASD;

    /// <summary>Service class (humidity exposure) — EC5 / TR.</summary>
    [ObservableProperty] private eServiceClass serviceClass = eServiceClass.ServiceClass1;

    /// <summary>Moisture (dry/wet) service condition — US.</summary>
    [ObservableProperty] private eMoistureContentCondition moistureCondition = eMoistureContentCondition.DryServiceConditon;

    /// <summary>Default load-duration class applied to members (EC5 / TR k_mod selection).</summary>
    [ObservableProperty] private eLoadDurationClass defaultLoadDuration = eLoadDurationClass.MediumTermAction;

    /// <summary>When true, the partial/modification factors below override the code defaults.</summary>
    [ObservableProperty] private bool factorsModified;

    /// <summary>EC5 material partial factor γ_M.</summary>
    [ObservableProperty] private double partialFactor = 1.3;

    /// <summary>EC5 modification factor k_mod.</summary>
    [ObservableProperty] private double modificationFactor = 0.8;

    /// <summary>US load-duration factor C_D (NDS Table 2.3.2).</summary>
    [ObservableProperty] private double loadDurationFactor = 1.0;

    /// <summary>US time-effect factor λ (LRFD).</summary>
    [ObservableProperty] private double timeEffectFactor = 1.0;
}
