using StructuralLoads;

namespace StructuralLoads.Generators
{
    /// <summary>A single factored action inside a combination clause (e.g. 1.35·Dead).</summary>
    public readonly record struct LoadTerm(eLoadNature Nature, double Factor);

    /// <summary>
    /// An "either/or" set of mutually-exclusive options within a clause, such as ASCE's
    /// "(Lr or S or R)". The expander emits one combination per present option.
    /// </summary>
    public sealed class LoadAlternativeGroup
    {
        public LoadAlternativeGroup(bool required, params LoadTerm[] options)
        {
            Required = required;
            Options = options;
        }

        /// <summary>The mutually-exclusive options; the governing one is found by taking the worst case.</summary>
        public IReadOnlyList<LoadTerm> Options { get; }

        /// <summary>
        /// When <c>true</c> the combination only exists if at least one option acts on the member
        /// (a leading action). When <c>false</c> the group is an accompanying action that is simply
        /// dropped if none of its options is present.
        /// </summary>
        public bool Required { get; }
    }

    /// <summary>
    /// A declarative description of one code combination: the always-present fixed terms (typically
    /// the permanent action) plus zero or more alternative groups. The base generator turns each
    /// clause into concrete <see cref="LoadCombination"/>s.
    /// </summary>
    public sealed class CombinationClause
    {
        public CombinationClause(string name, eLimitState limitState,
                                 IReadOnlyList<LoadTerm> fixedTerms,
                                 params LoadAlternativeGroup[] alternatives)
        {
            Name = name;
            LimitState = limitState;
            FixedTerms = fixedTerms;
            Alternatives = alternatives;
        }

        public string Name { get; }
        public eLimitState LimitState { get; }
        public IReadOnlyList<LoadTerm> FixedTerms { get; }
        public IReadOnlyList<LoadAlternativeGroup> Alternatives { get; }
    }
}
