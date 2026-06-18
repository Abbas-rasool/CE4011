using System.Collections.Generic;
using System.Linq;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Detects a topologically disconnected model: two or more groups of nodes
    /// that share no element. Each group is individually solvable, so the
    /// assembled matrix may not look singular — this defect is only visible from
    /// the connectivity graph, not from the determinant.
    /// </summary>
    public class ConnectivityRule : IModelValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(StructureModel model)
        {
            if (model.Elements.Count == 0)
                yield break; // nothing to connect; handled elsewhere

            // Union-find over the nodes that participate in elements.
            var parent = new Dictionary<int, int>();

            int Find(int x)
            {
                if (!parent.ContainsKey(x)) parent[x] = x;
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }
                return x;
            }

            void Union(int a, int b)
            {
                int ra = Find(a), rb = Find(b);
                if (ra != rb) parent[ra] = rb;
            }

            foreach (var element in model.Elements)
                Union(element.StartNode.Id, element.EndNode.Id);

            var components = parent.Keys
                .GroupBy(Find)
                .Select(g => g.OrderBy(id => id).ToList())
                .OrderBy(c => c.First())
                .ToList();

            if (components.Count > 1)
            {
                string parts = string.Join("; ",
                    components.Select(c => "{" + string.Join(", ", c) + "}"));

                yield return ValidationMessage.Error(
                    $"Model is split into {components.Count} disconnected parts (node groups {parts}). " +
                    "These are separate structures — connect them with an element or analyze them separately.");
            }
        }
    }
}
