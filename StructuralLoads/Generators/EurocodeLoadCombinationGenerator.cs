using StructuralLoads;

namespace StructuralLoads.Generators
{
    /// <summary>
    /// Eurocode (EN 1990) load combinations for EC5 timber design. The ULS uses Eq. 6.10 of the
    /// persistent &amp; transient design situation – each variable action leads in turn while the others
    /// accompany at γ_Q·ψ0. Eq. 6.10 envelopes the optional 6.10a/6.10b pair and is always permitted,
    /// so it is the form generated here. The seismic situation uses Eq. 6.12b, and the three SLS
    /// combinations (characteristic, frequent, quasi-permanent) follow EN 1990 §6.5.3.
    /// </summary>
    public sealed class EurocodeLoadCombinationGenerator : LoadCombinationGeneratorBase
    {
        private const double GammaG = 1.35;     // γ_G, permanent unfavourable
        private const double GammaGfav = 1.00;  // γ_G, permanent favourable (uplift / stability)
        private const double GammaQ = 1.50;     // γ_Q, variable unfavourable

        private static readonly eLoadNature[] VariableActions =
            { eLoadNature.Live, eLoadNature.Snow, eLoadNature.Wind };

        public override eLoadCode Code => eLoadCode.EN1990;

        protected override IEnumerable<CombinationClause> BuildClauses(LoadCombinationRequest request)
        {
            var psi = request.PsiFactors;
            var present = VariableActions.Where(request.IsPresent).ToList();

            // ---- ULS – Eq. 6.10, persistent & transient ----
            foreach (var leading in present)
            {
                yield return Persistent(leading, present, psi, GammaG, "EN 1990 6.10");

                if (leading == eLoadNature.Wind)   // permanent favourable variant captures uplift
                    yield return Persistent(leading, present, psi, GammaGfav, "EN 1990 6.10 G_inf");
            }

            if (present.Count == 0)
                yield return new CombinationClause("EN 1990 6.10", eLimitState.Ultimate, Dead(GammaG));

            // ---- ULS – Eq. 6.12b, seismic: G + E + Σ ψ2·Q ----
            if (request.IncludeSeismic && request.IsPresent(eLoadNature.Seismic))
                yield return Combo("EN 1990 6.12b seismic", eLimitState.UltimateSeismic, GammaGfav,
                    Term(eLoadNature.Seismic, 1.0),
                    CompanionsAt(present, n => psi.Psi2(n)));

            if (!request.IncludeServiceability) yield break;

            // ---- SLS – characteristic, frequent, quasi-permanent ----
            foreach (var leading in present)
            {
                yield return Combo($"EN 1990 char – {LoadCombination.Symbol(leading)}",
                    eLimitState.ServiceabilityCharacteristic, 1.0,
                    Term(leading, 1.0),
                    CompanionsAt(present.Where(n => n != leading), n => psi.Psi0(n)));

                yield return Combo($"EN 1990 freq – {LoadCombination.Symbol(leading)}",
                    eLimitState.ServiceabilityFrequent, 1.0,
                    Term(leading, psi.Psi1(leading)),
                    CompanionsAt(present.Where(n => n != leading), n => psi.Psi2(n)));
            }

            yield return Combo("EN 1990 quasi-perm", eLimitState.ServiceabilityQuasiPermanent, 1.0,
                leading: null,
                CompanionsAt(present, n => psi.Psi2(n)));
        }

        // Eq. 6.10 with the chosen permanent factor: γ_G·G + γ_Q·Q_lead + Σ γ_Q·ψ0,i·Q_i
        private static CombinationClause Persistent(eLoadNature leading, IReadOnlyList<eLoadNature> present,
                                                    PsiFactorTable psi, double gammaG, string tag) =>
            Combo($"{tag} – {LoadCombination.Symbol(leading)} leading", eLimitState.Ultimate, gammaG,
                Term(leading, GammaQ),
                CompanionsAt(present.Where(n => n != leading), n => GammaQ * psi.Psi0(n)));

        /// <summary>Assembles a clause from a permanent factor, an optional leading action and companions.</summary>
        private static CombinationClause Combo(string name, eLimitState limitState, double deadFactor,
                                               LoadTerm? leading, IEnumerable<LoadAlternativeGroup> companions)
        {
            var groups = new List<LoadAlternativeGroup>();
            if (leading is LoadTerm lead) groups.Add(Leading(lead));
            groups.AddRange(companions);

            return new CombinationClause(name, limitState, Dead(deadFactor), groups.ToArray());
        }

        /// <summary>One single-load companion group per action whose computed factor is non-zero.</summary>
        private static IEnumerable<LoadAlternativeGroup> CompanionsAt(IEnumerable<eLoadNature> actions,
                                                                      Func<eLoadNature, double> factor) =>
            actions
                .Select(n => (Nature: n, Factor: factor(n)))
                .Where(x => x.Factor != 0.0)
                .Select(x => Companion(Term(x.Nature, x.Factor)))
                .ToList();
    }
}
