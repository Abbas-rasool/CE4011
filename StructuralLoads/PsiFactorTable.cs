namespace StructuralLoads
{
    /// <summary>
    /// The combination factors ψ0/ψ1/ψ2 that Eurocode (EN 1990, Table A1.1) uses to reduce the
    /// magnitude of accompanying variable actions. Defaults follow the EN 1990 recommended set
    /// documented for the project (imposed cat. A/B, snow at sites ≤ 1000 m, wind).
    /// </summary>
    public sealed class PsiFactorTable
    {
        /// <summary>The three combination factors for a single variable action.</summary>
        public readonly record struct PsiSet(double Psi0, double Psi1, double Psi2);

        private readonly IReadOnlyDictionary<eLoadNature, PsiSet> _values;

        public PsiFactorTable(IReadOnlyDictionary<eLoadNature, PsiSet> values) => _values = values;

        /// <summary>EN 1990 recommended ψ values used unless the caller supplies its own table.</summary>
        public static PsiFactorTable Default { get; } = new(new Dictionary<eLoadNature, PsiSet>
        {
            [eLoadNature.Live]     = new PsiSet(0.7, 0.5, 0.3),
            [eLoadNature.RoofLive] = new PsiSet(0.0, 0.0, 0.0),
            [eLoadNature.Snow]     = new PsiSet(0.5, 0.2, 0.0),
            [eLoadNature.Wind]     = new PsiSet(0.6, 0.2, 0.0),
            [eLoadNature.Rain]     = new PsiSet(0.0, 0.0, 0.0)
        });

        /// <summary>All three factors for <paramref name="nature"/>; zero when not tabulated.</summary>
        public PsiSet For(eLoadNature nature) =>
            _values.TryGetValue(nature, out var set) ? set : new PsiSet(0.0, 0.0, 0.0);

        public double Psi0(eLoadNature nature) => For(nature).Psi0;
        public double Psi1(eLoadNature nature) => For(nature).Psi1;
        public double Psi2(eLoadNature nature) => For(nature).Psi2;
    }
}
