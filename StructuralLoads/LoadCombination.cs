namespace StructuralLoads
{
    /// <summary>
    /// An immutable, fully-expanded load combination: a named linear superposition of load
    /// natures with their partial factors, tagged with the code and limit state it belongs to.
    /// </summary>
    public sealed class LoadCombination
    {
        #region Symbols
        private static readonly IReadOnlyDictionary<eLoadNature, string> _symbols =
            new Dictionary<eLoadNature, string>
            {
                [eLoadNature.Dead] = "D",
                [eLoadNature.Live] = "L",
                [eLoadNature.RoofLive] = "Lr",
                [eLoadNature.Snow] = "S",
                [eLoadNature.Wind] = "W",
                [eLoadNature.Rain] = "R",
                [eLoadNature.Seismic] = "E",
                [eLoadNature.Thermal] = "T"
            };

        /// <summary>Short structural symbol for a load nature (e.g. Snow → "S").</summary>
        public static string Symbol(eLoadNature nature) =>
            _symbols.TryGetValue(nature, out var s) ? s : nature.ToString();
        #endregion

        #region CTOR
        public LoadCombination(string name, eLoadCode code, eLimitState limitState,
                               IReadOnlyDictionary<eLoadNature, double> factors)
        {
            Name = name;
            Code = code;
            LimitState = limitState;
            Factors = factors;
        }
        #endregion

        #region Properties
        /// <summary>Human-readable identifier, e.g. "LRFD 2 (S)" or "EN 1990 6.10 – W leading".</summary>
        public string Name { get; }

        /// <summary>Loading / combination standard this combination was generated for.</summary>
        public eLoadCode Code { get; }

        /// <summary>Limit state / design situation.</summary>
        public eLimitState LimitState { get; }

        /// <summary>Partial factor applied to each participating load nature.</summary>
        public IReadOnlyDictionary<eLoadNature, double> Factors { get; }
        #endregion

        #region Public Methods
        /// <summary>Returns the factor applied to <paramref name="nature"/>, or 0 when it does not participate.</summary>
        public double FactorFor(eLoadNature nature) =>
            Factors.TryGetValue(nature, out var f) ? f : 0.0;

        public override string ToString()
        {
            var terms = Factors
                .Where(kv => kv.Value != 0.0)
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Value:0.##}·{Symbol(kv.Key)}");

            return $"{Name} [{LimitState}]: {string.Join(" + ", terms)}";
        }
        #endregion
    }
}
