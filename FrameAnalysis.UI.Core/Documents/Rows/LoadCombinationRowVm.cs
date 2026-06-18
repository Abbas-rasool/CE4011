using CommunityToolkit.Mvvm.ComponentModel;
using StructuralLoads;

namespace FrameAnalysis.UI.Core.Documents.Rows;

/// <summary>
/// An editable load combination: a name, a limit state, and the partial factor applied to each
/// load nature. Rows are first populated from the design code's combination set (see
/// <c>LoadCombinationService</c>) and are then freely editable by the user — add, delete, rename,
/// or change factors. The design envelope (Phase 2) reads the ULS rows from this collection.
/// </summary>
public partial class LoadCombinationRowVm : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUltimate))]
    private eLimitState limitState = eLimitState.Ultimate;

    // Partial factor per nature (0 = does not participate).
    [ObservableProperty] private double dead;
    [ObservableProperty] private double live;
    [ObservableProperty] private double roofLive;
    [ObservableProperty] private double snow;
    [ObservableProperty] private double wind;
    [ObservableProperty] private double rain;
    [ObservableProperty] private double seismic;
    [ObservableProperty] private double thermal;

    /// <summary>True for strength (ULS) combinations the design envelope checks.</summary>
    public bool IsUltimate => LimitState is eLimitState.Ultimate or eLimitState.UltimateSeismic;

    /// <summary>Factor applied to <paramref name="nature"/> (0 if it does not participate).</summary>
    public double FactorFor(eLoadNature nature) => nature switch
    {
        eLoadNature.Dead => Dead,
        eLoadNature.Live => Live,
        eLoadNature.RoofLive => RoofLive,
        eLoadNature.Snow => Snow,
        eLoadNature.Wind => Wind,
        eLoadNature.Rain => Rain,
        eLoadNature.Seismic => Seismic,
        eLoadNature.Thermal => Thermal,
        _ => 0.0
    };

    /// <summary>Copies a generated combination's name, limit state, and factors into this row.</summary>
    public void LoadFromCombination(LoadCombination c)
    {
        Name = c.Name;
        LimitState = c.LimitState;
        Dead = c.FactorFor(eLoadNature.Dead);
        Live = c.FactorFor(eLoadNature.Live);
        RoofLive = c.FactorFor(eLoadNature.RoofLive);
        Snow = c.FactorFor(eLoadNature.Snow);
        Wind = c.FactorFor(eLoadNature.Wind);
        Rain = c.FactorFor(eLoadNature.Rain);
        Seismic = c.FactorFor(eLoadNature.Seismic);
        Thermal = c.FactorFor(eLoadNature.Thermal);
    }

    public static LoadCombinationRowVm FromCombination(LoadCombination c)
    {
        var row = new LoadCombinationRowVm();
        row.LoadFromCombination(c);
        return row;
    }
}
