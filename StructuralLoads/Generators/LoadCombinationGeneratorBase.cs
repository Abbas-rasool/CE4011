using StructuralLoads;

namespace StructuralLoads.Generators
{
    /// <summary>
    /// Shares the mechanics common to every code: expanding declarative <see cref="CombinationClause"/>s
    /// (which may contain "either/or" alternative groups) into concrete <see cref="LoadCombination"/>s,
    /// pruning them to the loads that actually act on the member, and removing duplicates. Concrete
    /// generators only supply the code-specific clauses via <see cref="BuildClauses"/>.
    /// </summary>
    public abstract class LoadCombinationGeneratorBase : ILoadCombinationGenerator
    {
        public abstract eLoadCode Code { get; }

        /// <summary>Code-specific data: the clauses to expand for this request.</summary>
        protected abstract IEnumerable<CombinationClause> BuildClauses(LoadCombinationRequest request);

        public IReadOnlyList<LoadCombination> Generate(LoadCombinationRequest request)
        {
            var combinations = new List<LoadCombination>();
            var seen = new HashSet<string>();

            foreach (var clause in BuildClauses(request))
            {
                foreach (var combo in ExpandClause(clause, request))
                {
                    // De-duplicate combinations that collapse onto each other once pruned.
                    if (seen.Add(Signature(combo)))
                        combinations.Add(combo);
                }
            }

            return combinations;
        }

        #region Clause authoring helpers (for derived generators)
        protected static LoadTerm Term(eLoadNature nature, double factor) => new(nature, factor);

        /// <summary>The permanent action carried by (almost) every combination.</summary>
        protected static IReadOnlyList<LoadTerm> Dead(double factor) =>
            new[] { new LoadTerm(eLoadNature.Dead, factor) };

        /// <summary>A leading (required) alternative group – the combination is void without it.</summary>
        protected static LoadAlternativeGroup Leading(params LoadTerm[] options) => new(required: true, options);

        /// <summary>An accompanying (optional) alternative group – dropped if none of its options is present.</summary>
        protected static LoadAlternativeGroup Companion(params LoadTerm[] options) => new(required: false, options);
        #endregion

        #region Expansion
        /// <summary>
        /// Expands a clause into one combination per selectable alternative, applying the present-loads
        /// filter. Yields nothing when a required group has no present option.
        /// </summary>
        private IEnumerable<LoadCombination> ExpandClause(CombinationClause clause, LoadCombinationRequest request)
        {
            // Each partial combination under construction = its accumulated terms + a label suffix.
            var partials = new List<(Dictionary<eLoadNature, double> Terms, List<string> Labels)>
            {
                (StartingTerms(clause, request), new List<string>())
            };

            foreach (var group in clause.Alternatives)
            {
                var presentOptions = group.Options.Where(o => request.IsPresent(o.Nature)).ToList();

                if (presentOptions.Count == 0)
                {
                    if (group.Required)
                        yield break;   // a mandatory leading action is absent → the whole combination is void
                    continue;          // an accompanying group is absent → simply omit it
                }

                var expanded = new List<(Dictionary<eLoadNature, double>, List<string>)>();
                foreach (var (terms, labels) in partials)
                {
                    foreach (var option in presentOptions)
                    {
                        var nextTerms = new Dictionary<eLoadNature, double>(terms);
                        Accumulate(nextTerms, option.Nature, option.Factor);

                        var nextLabels = new List<string>(labels);
                        if (group.Options.Count > 1)
                            nextLabels.Add(LoadCombination.Symbol(option.Nature));

                        expanded.Add((nextTerms, nextLabels));
                    }
                }
                partials = expanded;
            }

            foreach (var (terms, labels) in partials)
            {
                // Once accompanying actions are pruned away a clause can degenerate to dead-load only;
                // drop those, unless the clause was a genuine permanent-only combination to begin with.
                if (request.PresentLoads is not null
                    && clause.Alternatives.Count > 0
                    && !terms.Keys.Any(n => n != eLoadNature.Dead))
                {
                    continue;
                }

                var name = labels.Count > 0 ? $"{clause.Name} ({string.Join("/", labels)})" : clause.Name;
                yield return new LoadCombination(name, Code, clause.LimitState, terms);
            }
        }

        private static Dictionary<eLoadNature, double> StartingTerms(CombinationClause clause, LoadCombinationRequest request)
        {
            var terms = new Dictionary<eLoadNature, double>();
            foreach (var term in clause.FixedTerms)
            {
                // Dead is always kept (its favourable/unfavourable value is part of the combination);
                // any other fixed term is pruned when it does not act on the member.
                if (term.Nature == eLoadNature.Dead || request.IsPresent(term.Nature))
                    Accumulate(terms, term.Nature, term.Factor);
            }
            return terms;
        }

        private static void Accumulate(Dictionary<eLoadNature, double> terms, eLoadNature nature, double factor) =>
            terms[nature] = terms.TryGetValue(nature, out var existing) ? existing + factor : factor;

        private static string Signature(LoadCombination combo)
        {
            var parts = combo.Factors
                .Where(kv => kv.Value != 0.0)
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{(int)kv.Key}:{kv.Value:0.####}");

            return $"{(int)combo.LimitState}|{string.Join(",", parts)}";
        }
        #endregion
    }
}
