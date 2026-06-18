namespace StructuralLoads
{
    /// <summary>
    /// Optional parameters that tune which combinations a generator produces. Every property has a
    /// sensible default, so callers may pass <c>null</c> to <see cref="LoadingCaseFactory"/>.
    /// </summary>
    public sealed class LoadCombinationRequest
    {
        /// <summary>
        /// Design method for codes that expose both (currently ASCE 7): ASD or LRFD.
        /// Ignored by EN 1990 and TBDY, which use a single limit-state format.
        /// </summary>
        public eLoadCombinationType DesignMethod { get; init; } = eLoadCombinationType.ASD;

        /// <summary>
        /// When supplied, only these load natures are assumed to act on the member; the combinations
        /// are pruned to them and de-duplicated. When <c>null</c>, the full canonical set is returned.
        /// </summary>
        public IReadOnlySet<eLoadNature>? PresentLoads { get; init; }

        /// <summary>Eurocode ψ combination factors. Defaults to <see cref="PsiFactorTable.Default"/>.</summary>
        public PsiFactorTable PsiFactors { get; init; } = PsiFactorTable.Default;

        /// <summary>Include serviceability limit-state combinations. Default <c>true</c>.</summary>
        public bool IncludeServiceability { get; init; } = true;

        /// <summary>Include seismic design-situation combinations. Default <c>true</c>.</summary>
        public bool IncludeSeismic { get; init; } = true;

        /// <summary>Shared default instance used when the caller does not supply a request.</summary>
        internal static LoadCombinationRequest Default { get; } = new();

        /// <summary>True when <paramref name="nature"/> acts on the member (always true with no filter).</summary>
        internal bool IsPresent(eLoadNature nature) =>
            PresentLoads is null || PresentLoads.Contains(nature);
    }
}
