using StructuralLoads;

namespace StructuralLoads.Generators
{
    /// <summary>
    /// ASCE 7-22 load combinations. Strength Design / LRFD comes from §2.3.1 (basic) and §2.3.6
    /// (seismic); Allowable Stress Design / ASD from §2.4.1 (basic) and §2.4.5 (seismic).
    /// The 7-22 snow factors are encoded (LRFD: leading 1.0S, companion 0.3S; ASD: 0.7S).
    /// Seismic E already bundles the vertical effect (Ev = 0.2·S_DS·D) per the code.
    /// </summary>
    public sealed class AmericanLoadCombinationGenerator : LoadCombinationGeneratorBase
    {
        public override eLoadCode Code => eLoadCode.ASCE7;

        protected override IEnumerable<CombinationClause> BuildClauses(LoadCombinationRequest request) =>
            request.DesignMethod == eLoadCombinationType.LRFD
                ? StrengthDesign(request)
                : AllowableStressDesign(request);

        // ASCE 7-22 §2.3 – Strength Design (LRFD)
        private static IEnumerable<CombinationClause> StrengthDesign(LoadCombinationRequest request)
        {
            yield return new CombinationClause("LRFD 1", eLimitState.Ultimate, Dead(1.4));

            yield return new CombinationClause("LRFD 2", eLimitState.Ultimate, Dead(1.2),
                Leading(Term(eLoadNature.Live, 1.6)),
                Companion(Term(eLoadNature.RoofLive, 0.5), Term(eLoadNature.Snow, 0.3), Term(eLoadNature.Rain, 0.5)));

            yield return new CombinationClause("LRFD 3", eLimitState.Ultimate, Dead(1.2),
                Leading(Term(eLoadNature.RoofLive, 1.6), Term(eLoadNature.Snow, 1.0), Term(eLoadNature.Rain, 1.6)),
                Companion(Term(eLoadNature.Live, 1.0), Term(eLoadNature.Wind, 0.5)));

            yield return new CombinationClause("LRFD 4", eLimitState.Ultimate, Dead(1.2),
                Leading(Term(eLoadNature.Wind, 1.0)),
                Companion(Term(eLoadNature.Live, 1.0)),
                Companion(Term(eLoadNature.RoofLive, 0.5), Term(eLoadNature.Snow, 0.3), Term(eLoadNature.Rain, 0.5)));

            yield return new CombinationClause("LRFD 5", eLimitState.Ultimate, Dead(0.9),
                Leading(Term(eLoadNature.Wind, 1.0)));

            if (!request.IncludeSeismic) yield break;

            // §2.3.6 – seismic
            yield return new CombinationClause("LRFD 6", eLimitState.UltimateSeismic, Dead(1.2),
                Leading(Term(eLoadNature.Seismic, 1.0)),
                Companion(Term(eLoadNature.Live, 1.0)),
                Companion(Term(eLoadNature.Snow, 0.2)));

            yield return new CombinationClause("LRFD 7", eLimitState.UltimateSeismic, Dead(0.9),
                Leading(Term(eLoadNature.Seismic, 1.0)));
        }

        // ASCE 7-22 §2.4 – Allowable Stress Design (ASD). Snow already reduced by 0.7 (e.g. 0.75·0.7 = 0.525).
        private static IEnumerable<CombinationClause> AllowableStressDesign(LoadCombinationRequest request)
        {
            yield return new CombinationClause("ASD 1", eLimitState.Ultimate, Dead(1.0));

            yield return new CombinationClause("ASD 2", eLimitState.Ultimate, Dead(1.0),
                Leading(Term(eLoadNature.Live, 1.0)));

            yield return new CombinationClause("ASD 3", eLimitState.Ultimate, Dead(1.0),
                Leading(Term(eLoadNature.RoofLive, 1.0), Term(eLoadNature.Snow, 0.7), Term(eLoadNature.Rain, 1.0)));

            yield return new CombinationClause("ASD 4", eLimitState.Ultimate, Dead(1.0),
                Leading(Term(eLoadNature.Live, 0.75)),
                Companion(Term(eLoadNature.RoofLive, 0.75), Term(eLoadNature.Snow, 0.525), Term(eLoadNature.Rain, 0.75)));

            yield return new CombinationClause("ASD 5", eLimitState.Ultimate, Dead(1.0),
                Leading(Term(eLoadNature.Wind, 0.6)));

            yield return new CombinationClause("ASD 6", eLimitState.Ultimate, Dead(1.0),
                Leading(Term(eLoadNature.Wind, 0.45)),   // 0.75 · 0.6W
                Companion(Term(eLoadNature.Live, 0.75)),
                Companion(Term(eLoadNature.RoofLive, 0.75), Term(eLoadNature.Snow, 0.525), Term(eLoadNature.Rain, 0.75)));

            yield return new CombinationClause("ASD 7", eLimitState.Ultimate, Dead(0.6),
                Leading(Term(eLoadNature.Wind, 0.6)));

            if (!request.IncludeSeismic) yield break;

            // §2.4.5 – seismic, E carries the 0.7 ASD factor
            yield return new CombinationClause("ASD 8", eLimitState.UltimateSeismic, Dead(1.0),
                Leading(Term(eLoadNature.Seismic, 0.7)));

            yield return new CombinationClause("ASD 9", eLimitState.UltimateSeismic, Dead(1.0),
                Leading(Term(eLoadNature.Seismic, 0.525)),   // 0.75 · 0.7E
                Companion(Term(eLoadNature.Live, 0.75)),
                Companion(Term(eLoadNature.Snow, 0.525)));   // 0.75 · 0.7S

            yield return new CombinationClause("ASD 10", eLimitState.UltimateSeismic, Dead(0.6),
                Leading(Term(eLoadNature.Seismic, 0.7)));
        }
    }
}
