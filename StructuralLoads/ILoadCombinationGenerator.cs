namespace StructuralLoads
{
    /// <summary>
    /// Strategy that produces the load combinations mandated by one loading standard.
    /// One implementation per <see cref="eLoadCode"/> keeps each code's rules isolated (SRP) and
    /// lets new codes be added without modifying existing ones or the factory (OCP). Consumers depend
    /// on this abstraction rather than the concrete generators (DIP).
    /// </summary>
    public interface ILoadCombinationGenerator
    {
        /// <summary>The loading standard this generator implements.</summary>
        eLoadCode Code { get; }

        /// <summary>Builds every load combination required by the code for the given request.</summary>
        IReadOnlyList<LoadCombination> Generate(LoadCombinationRequest request);
    }
}
