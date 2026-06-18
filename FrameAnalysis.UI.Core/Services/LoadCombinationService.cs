using System;
using System.Collections.Generic;
using FrameAnalysis.UI.Core.Documents;
using StructuralLoads;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>
/// Generates the code's load combinations for a document by feeding the present load natures and
/// the active design code into <see cref="LoadingCaseFactory"/>. The result is used to populate
/// the document's editable <see cref="LoadCombinationRowVm"/> collection; thereafter the user
/// owns it. Stateless and thread-safe.
/// </summary>
public sealed class LoadCombinationService
{
    private readonly LoadingCaseFactory _factory = new();

    /// <summary>The full combination set (all limit states) for the document's loads and code.</summary>
    public IReadOnlyList<LoadCombination> Generate(ProjectDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var request = new LoadCombinationRequest
        {
            PresentLoads = PresentNatures(document),
            DesignMethod = document.Design.UsDesignMethod
        };
        return _factory.Create(ToLoadCode(document.Design.Code), request);
    }

    /// <summary>The distinct load natures actually used by the document's loads (thermal loads
    /// imply <see cref="eLoadNature.Thermal"/>; settlements have no nature).</summary>
    public static IReadOnlySet<eLoadNature> PresentNatures(ProjectDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        var natures = new HashSet<eLoadNature>();
        foreach (var l in doc.NodalLoads) natures.Add(l.Nature);
        foreach (var d in doc.DistributedLoads) natures.Add(d.Nature);
        foreach (var p in doc.PointLoads) natures.Add(p.Nature);
        if (doc.TemperatureLoads.Count > 0) natures.Add(eLoadNature.Thermal);
        return natures;
    }

    /// <summary>Maps the material design code to the matching load-combination standard.</summary>
    public static eLoadCode ToLoadCode(eTimberCode code) => code switch
    {
        eTimberCode.US => eLoadCode.ASCE7,
        eTimberCode.EC5 => eLoadCode.EN1990,
        eTimberCode.TR => eLoadCode.TBDY,
        _ => eLoadCode.EN1990
    };
}
