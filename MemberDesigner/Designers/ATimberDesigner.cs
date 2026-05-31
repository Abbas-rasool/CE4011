using System.Collections.Concurrent;
using MemberDesigner.DesignChecks;
using MemberDesigner.Designers;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.Designers
{
    public class ATimberDesigner
    {
        #region CTOR
        public ATimberDesigner(TimberDesignCheckProvider checkProvider, TimberDesignCheckInputFactory inputFactory)
        {
            _checkProvider = checkProvider;
            _inputFactory = inputFactory;
        }
        #endregion

        #region Fields

        private TimberDesignCheckProvider _checkProvider;
        private TimberDesignCheckInputFactory _inputFactory;

        #endregion

        #region Private Methods
        private List<List<ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>>> BuildDependencyLevels(IList<ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>> checks)
        {
            var levels = new List<List<ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>>>();
            var remaining = new HashSet<ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>>(checks);
            var completed = new HashSet<eTimberDesignCheckType>();

            while (remaining.Count > 0)
            {
                var level = remaining.Where(check => check.Dependencies == null ||check.Dependencies.All(dep => completed.Contains(dep))).ToList();

                if (level.Count == 0)
                {
                    var remainingTypes = string.Join(", ", remaining.Select(c => c.CheckType));
                    throw new InvalidOperationException($"Circular dependency detected. Remaining: {remainingTypes}");
                }

                levels.Add(level);

                foreach (var check in level)
                {
                    completed.Add(check.CheckType);
                    remaining.Remove(check);
                }
            }

            return levels;
        }
        #endregion

        #region Public Methods
        public async Task<List<TimberDesignCheckData>> CheckDesignAsync(CancellationToken cancellationToken = default)
        {
            var checks = _checkProvider.ProvideChecks();
            var checkInputs = _inputFactory.PrepareAllCheckInputs();

            // 2. Create lookup for quick access
            var inputLookup = checkInputs.ToDictionary(x => x.CheckType, x => x);

            // 3. Store results as they complete
            var results = new ConcurrentDictionary<eTimberDesignCheckType, TimberDesignCheckData>();

            // 4. Build the dependency levels
            var levels = BuildDependencyLevels(checks);

            // 5. Process each level sequentially
            foreach (var level in levels)
            {
                var levelTasks = new List<Task>();

                // 6. All checks in this level can run in parallel
                foreach (var check in level)
                {
                    var task = Task.Run(() =>
                    {
                        // 7. Get dependencies (already computed from previous levels)
                        var dependencyResults = check.Dependencies?
                            .Where(d => results.ContainsKey(d))
                            .Select(d => results[d])
                            .ToArray() ?? Array.Empty<TimberDesignCheckData>();



                        // 8. Execute the check
                        var result = check.PerformCheck(
                            inputLookup[check.CheckType],
                            dependencyResults);

                        // 9. Store the result
                        results[check.CheckType] = result;

                    }, cancellationToken);

                    levelTasks.Add(task);
                }

                // 10. Wait for ALL checks in this level to complete
                await Task.WhenAll(levelTasks).ConfigureAwait(false);
            }

            // 11. Return all results
            return results.Values.ToList();
        }
        #endregion
    }
}
