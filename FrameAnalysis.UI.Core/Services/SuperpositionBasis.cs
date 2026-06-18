using System;
using System.Collections.Generic;
using FrameAnalysis.UI.Core.Documents.Rows;
using StructuralLoads;

namespace FrameAnalysis.UI.Core.Services;

/// <summary>
/// The per-load-case member end forces from a linear analysis, ready to be superposed into any
/// combination's demands. Holds one element→end-force map per load nature, plus an optional
/// settlement map (settlements act at factor 1.0 in every combination). Because the analysis is
/// linear and first-order, a combination's member demand is exactly the factor-weighted sum of
/// the per-nature forces.
/// </summary>
public sealed class SuperpositionBasis
{
    private readonly IReadOnlyDictionary<eLoadNature, IReadOnlyDictionary<int, double[]>> _perNature;
    private readonly IReadOnlyDictionary<int, double[]>? _settlement;

    public SuperpositionBasis(
        IReadOnlyDictionary<eLoadNature, IReadOnlyDictionary<int, double[]>> perNature,
        IReadOnlyDictionary<int, double[]>? settlement)
    {
        _perNature = perNature ?? throw new ArgumentNullException(nameof(perNature));
        _settlement = settlement;
    }

    /// <summary>
    /// Local end forces for <paramref name="elementId"/> under <paramref name="combo"/>:
    /// Σ factor·forces_nature, plus settlement at factor 1.0. Returns a zero 6-vector when the
    /// element has no contributing loads.
    /// </summary>
    public double[] CombinedEndForces(int elementId, LoadCombinationRowVm combo)
    {
        ArgumentNullException.ThrowIfNull(combo);
        double[]? acc = null;

        foreach (var (nature, byElement) in _perNature)
        {
            double factor = combo.FactorFor(nature);
            if (factor == 0.0) continue;
            if (!byElement.TryGetValue(elementId, out double[]? v)) continue;
            acc ??= new double[v.Length];
            for (int i = 0; i < v.Length; i++) acc[i] += factor * v[i];
        }

        if (_settlement is not null && _settlement.TryGetValue(elementId, out double[]? s))
        {
            acc ??= new double[s.Length];
            for (int i = 0; i < s.Length; i++) acc[i] += s[i];
        }

        return acc ?? new double[6];
    }
}
