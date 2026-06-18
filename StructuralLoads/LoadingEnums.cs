namespace StructuralLoads
{
    /// <summary>
    /// The nature (physical source) of a structural action that participates in a load combination.
    /// Symbols follow common structural notation: Dead = G/D, Live = Q/L, Snow = S, Wind = W, Seismic = E.
    /// </summary>
    public enum eLoadNature : byte
    {
        /// <summary>Permanent / dead load (G in EC &amp; TS, D in ASCE).</summary>
        Dead = 0,

        /// <summary>Variable / imposed / occupancy live load (Q in EC &amp; TS, L in ASCE).</summary>
        Live = 1,

        /// <summary>Roof live load (Lr in ASCE).</summary>
        RoofLive = 2,

        /// <summary>Snow load (S).</summary>
        Snow = 3,

        /// <summary>Wind load (W).</summary>
        Wind = 4,

        /// <summary>Rain / ponding load (R in ASCE).</summary>
        Rain = 5,

        /// <summary>Seismic / earthquake load (E).</summary>
        Seismic = 6,

        /// <summary>Self-straining thermal action – uniform change and/or gradient (T).</summary>
        Thermal = 7
    }

    /// <summary>
    /// The limit state and, where relevant, the design situation a combination belongs to.
    /// </summary>
    public enum eLimitState : byte
    {
        /// <summary>Ultimate limit state – persistent &amp; transient (strength) design situation.</summary>
        Ultimate = 0,

        /// <summary>Ultimate limit state – seismic design situation.</summary>
        UltimateSeismic = 1,

        /// <summary>Ultimate limit state – accidental design situation.</summary>
        UltimateAccidental = 2,

        /// <summary>Serviceability – characteristic (rare) combination.</summary>
        ServiceabilityCharacteristic = 3,

        /// <summary>Serviceability – frequent combination.</summary>
        ServiceabilityFrequent = 4,

        /// <summary>Serviceability – quasi-permanent combination.</summary>
        ServiceabilityQuasiPermanent = 5
    }

    /// <summary>
    /// The loading / combination standard a set of load combinations is generated from. This is the
    /// material-independent counterpart of a material design code (e.g. timber's eTimberCode):
    /// the same combinations apply whether the member is timber, steel or concrete.
    /// </summary>
    public enum eLoadCode : byte
    {
        /// <summary>ASCE 7-22 (LRFD §2.3 / ASD §2.4).</summary>
        ASCE7 = 0,

        /// <summary>EN 1990 (6.10, 6.12b, SLS).</summary>
        EN1990 = 1,

        /// <summary>TBDY 2018 / YDKT (Turkish).</summary>
        TBDY = 2
    }

    /// <summary>
    /// Design method for codes that expose both formats (currently ASCE 7): allowable-stress or
    /// load-and-resistance-factor design. Ignored by EN 1990 and TBDY, which use a single format.
    /// </summary>
    public enum eLoadCombinationType : byte
    {
        LRFD = 0,
        ASD = 1
    }
}
