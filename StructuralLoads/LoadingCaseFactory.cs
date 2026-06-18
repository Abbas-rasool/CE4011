using StructuralLoads.Generators;

namespace StructuralLoads
{
    /// <summary>
    /// Entry point for obtaining the load combinations of a loading standard. Given an
    /// <see cref="eLoadCode"/> it delegates to the matching <see cref="ILoadCombinationGenerator"/>:
    /// <list type="bullet">
    ///   <item><description><see cref="eLoadCode.ASCE7"/> → ASCE 7-22 (LRFD §2.3 / ASD §2.4).</description></item>
    ///   <item><description><see cref="eLoadCode.EN1990"/> → EN 1990 (6.10, 6.12b, SLS).</description></item>
    ///   <item><description><see cref="eLoadCode.TBDY"/> → TBDY 2018 / YDKT.</description></item>
    /// </list>
    /// Supporting a new code only requires registering another generator – no change to this class (OCP),
    /// and the factory depends solely on the <see cref="ILoadCombinationGenerator"/> abstraction (DIP).
    /// </summary>
    public sealed class LoadingCaseFactory
    {
        private readonly IReadOnlyDictionary<eLoadCode, ILoadCombinationGenerator> _generators;

        /// <summary>Wires the factory with the built-in ASCE 7, EN 1990 and TBDY generators.</summary>
        public LoadingCaseFactory()
            : this(new ILoadCombinationGenerator[]
            {
                new AmericanLoadCombinationGenerator(),
                new EurocodeLoadCombinationGenerator(),
                new TurkishLoadCombinationGenerator()
            })
        {
        }

        /// <summary>Wires the factory from an explicit set of generators (dependency injection / testing).</summary>
        public LoadingCaseFactory(IEnumerable<ILoadCombinationGenerator> generators)
        {
            ArgumentNullException.ThrowIfNull(generators);
            _generators = generators.ToDictionary(generator => generator.Code);
        }

        /// <summary>True when a generator is registered for <paramref name="code"/>.</summary>
        public bool Supports(eLoadCode code) => _generators.ContainsKey(code);

        /// <summary>
        /// Produces every load combination the <paramref name="code"/> requires.
        /// </summary>
        /// <param name="code">Loading standard to generate combinations for.</param>
        /// <param name="request">
        /// Optional tuning (design method, present loads, ψ factors, limit-state toggles).
        /// Uses sensible defaults when <c>null</c>.
        /// </param>
        /// <exception cref="NotSupportedException">No generator is registered for <paramref name="code"/>.</exception>
        public IReadOnlyList<LoadCombination> Create(eLoadCode code, LoadCombinationRequest? request = null)
        {
            if (!_generators.TryGetValue(code, out var generator))
                throw new NotSupportedException($"No load-combination generator is registered for code '{code}'.");

            return generator.Generate(request ?? LoadCombinationRequest.Default);
        }
    }
}
