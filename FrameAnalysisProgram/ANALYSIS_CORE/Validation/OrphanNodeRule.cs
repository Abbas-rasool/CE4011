using System.Collections.Generic;
using System.Linq;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Flags nodes that are not attached to any element. Such a node has no
    /// stiffness and produces a singular system.
    /// </summary>
    public class OrphanNodeRule : IModelValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(StructureModel model)
        {
            var connected = new HashSet<int>();
            foreach (var element in model.Elements)
            {
                connected.Add(element.StartNode.Id);
                connected.Add(element.EndNode.Id);
            }

            var orphans = model.Nodes
                .Where(n => !connected.Contains(n.Id))
                .Select(n => n.Id)
                .ToList();

            if (orphans.Count > 0)
            {
                yield return ValidationMessage.Error(
                    $"Node(s) {string.Join(", ", orphans)} are not connected to any element " +
                    "(no stiffness). Remove them or connect them with an element.");
            }
        }
    }
}
