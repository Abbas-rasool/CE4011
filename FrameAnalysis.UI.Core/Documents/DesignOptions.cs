using System.Collections.Generic;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Documents;

/// <summary>
/// Curated option lists for the Design UI — only the values the timber design checks actually
/// support, so the dropdowns don't offer choices the backend can't handle (e.g. LVL, OSB,
/// plywood, CLT). Solid-member scope ⇒ solid timber and glulam only.
/// </summary>
public static class DesignOptions
{
    public static IReadOnlyList<eTimberMaterialType> MaterialTypes { get; } = new[]
    {
        eTimberMaterialType.SolidTimber,
        eTimberMaterialType.GluedLaminatedTimber,
    };
}
