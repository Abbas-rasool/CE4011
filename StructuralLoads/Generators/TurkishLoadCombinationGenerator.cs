using StructuralLoads;

namespace StructuralLoads.Generators
{
    /// <summary>
    /// Turkish (TBDY 2018 / YDKT) load combinations over G, Q, S, W, E. The persistent/transient set
    /// mirrors the Eurocode partial-factor format (companion factor 1.05 = 1.5 × ψ0, with ψ0 = 0.7).
    /// The seismic rows (10–11) keep E as a single nature; the actual earthquake action still has to
    /// be resolved with the TBDY directional rule (±Ex ± 0.3Ey) before these factors are applied.
    /// </summary>
    public sealed class TurkishLoadCombinationGenerator : LoadCombinationGeneratorBase
    {
        public override eLoadCode Code => eLoadCode.TBDY;

        protected override IEnumerable<CombinationClause> BuildClauses(LoadCombinationRequest request)
        {
            // ---- Ultimate limit state (strength / dayanım) ----
            yield return new CombinationClause("TS ULS 1", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Live, 1.5)));

            yield return new CombinationClause("TS ULS 2", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Snow, 1.5)));

            yield return new CombinationClause("TS ULS 3", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Wind, 1.5)));

            yield return new CombinationClause("TS ULS 4", eLimitState.Ultimate, Dead(0.90),
                Leading(Term(eLoadNature.Wind, 1.5)));

            yield return new CombinationClause("TS ULS 5", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Live, 1.5)),
                Companion(Term(eLoadNature.Snow, 1.05)));

            yield return new CombinationClause("TS ULS 6", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Snow, 1.5)),
                Companion(Term(eLoadNature.Live, 1.05)));

            yield return new CombinationClause("TS ULS 7", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Live, 1.5)),
                Companion(Term(eLoadNature.Snow, 1.05)),
                Companion(Term(eLoadNature.Wind, 1.05)));

            yield return new CombinationClause("TS ULS 8", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Snow, 1.5)),
                Companion(Term(eLoadNature.Live, 1.05)),
                Companion(Term(eLoadNature.Wind, 1.05)));

            yield return new CombinationClause("TS ULS 9", eLimitState.Ultimate, Dead(1.35),
                Leading(Term(eLoadNature.Wind, 1.5)),
                Companion(Term(eLoadNature.Live, 1.05)),
                Companion(Term(eLoadNature.Snow, 1.05)));

            // ---- Ultimate limit state (seismic) ----
            if (request.IncludeSeismic)
            {
                yield return new CombinationClause("TS ULS 10", eLimitState.UltimateSeismic, Dead(1.0),
                    Leading(Term(eLoadNature.Seismic, 1.0)),
                    Companion(Term(eLoadNature.Live, 1.0)),
                    Companion(Term(eLoadNature.Snow, 0.4)));

                yield return new CombinationClause("TS ULS 11", eLimitState.UltimateSeismic, Dead(0.9),
                    Leading(Term(eLoadNature.Seismic, 1.0)));
            }

            if (!request.IncludeServiceability) yield break;

            // ---- Serviceability limit state (kullanılabilirlik) ----
            yield return new CombinationClause("TS SLS 1", eLimitState.ServiceabilityCharacteristic, Dead(1.0),
                Leading(Term(eLoadNature.Live, 1.0)));

            yield return new CombinationClause("TS SLS 2", eLimitState.ServiceabilityCharacteristic, Dead(1.0),
                Leading(Term(eLoadNature.Snow, 1.0)));

            yield return new CombinationClause("TS SLS 3", eLimitState.ServiceabilityCharacteristic, Dead(1.0),
                Leading(Term(eLoadNature.Wind, 1.0)));

            yield return new CombinationClause("TS SLS 4", eLimitState.ServiceabilityCharacteristic, Dead(1.0),
                Leading(Term(eLoadNature.Live, 1.0)),
                Companion(Term(eLoadNature.Snow, 0.7)),
                Companion(Term(eLoadNature.Wind, 0.7)));

            yield return new CombinationClause("TS SLS 5", eLimitState.ServiceabilityCharacteristic, Dead(1.0),
                Leading(Term(eLoadNature.Snow, 1.0)),
                Companion(Term(eLoadNature.Live, 0.7)),
                Companion(Term(eLoadNature.Wind, 0.7)));

            yield return new CombinationClause("TS SLS 6", eLimitState.ServiceabilityCharacteristic, Dead(1.0),
                Leading(Term(eLoadNature.Wind, 1.0)),
                Companion(Term(eLoadNature.Live, 0.7)),
                Companion(Term(eLoadNature.Snow, 0.7)));
        }
    }
}
