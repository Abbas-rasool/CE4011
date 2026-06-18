using System.Collections.Generic;
using System.Linq;
using FrameAnalysisProgram.STRUCTURAL_MODEL;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// A cheap determinacy "screen" (not the authoritative test — the stiffness
    /// factorization is). Reports whether the static determinacy count suggests a
    /// mechanism (negative), a determinate structure (zero), or redundancy
    /// (positive). It is computed for pure-frame and pure-truss models, where the
    /// count is exact; for mixed models it is skipped to avoid a misleading number.
    /// </summary>
    public class DeterminacyRule : IModelValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(StructureModel model)
        {
            int memberCount = model.Elements.Count;
            if (memberCount == 0)
                yield break;

            int joints = model.Nodes.Count;
            bool allTruss = model.Elements.All(e => e is TrussElement2D);
            bool allFrame = model.Elements.All(e => e is FrameElement2D);

            if (allTruss)
            {
                // Planar truss: stable/determinate needs m + r = 2j (r = translational reactions).
                int r = model.Supports.Sum(s => (s.RestrainsUx ? 1 : 0) + (s.RestrainsUy ? 1 : 0));
                int dsi = memberCount + r - 2 * joints;
                yield return DescribeTruss(dsi);
            }
            else if (allFrame)
            {
                // Planar frame: DSI = 3m + r - 3j - c (c = released conditions from hinges).
                int r = model.Supports.Sum(s =>
                    (s.RestrainsUx ? 1 : 0) + (s.RestrainsUy ? 1 : 0) + (s.RestrainsRz ? 1 : 0));
                int c = model.Elements.OfType<FrameElement2D>().Sum(ReleasedConditions);
                int dsi = 3 * memberCount + r - 3 * joints - c;
                yield return DescribeFrame(dsi);
            }
            else
            {
                yield return ValidationMessage.Info(
                    "Mixed frame/truss model: skipping the determinacy count; the stiffness " +
                    "factorization is used as the authoritative stability check.");
            }
        }

        private static int ReleasedConditions(FrameElement2D element)
        {
            switch (element.Release)
            {
                case MomentRelease.Start:
                case MomentRelease.End:
                    return 1;
                case MomentRelease.Both:
                    return 2;
                default:
                    return 0;
            }
        }

        private static ValidationMessage DescribeFrame(int dsi)
        {
            if (dsi < 0)
                return ValidationMessage.Warning(
                    $"Determinacy screen: frame DSI = {dsi} (< 0) suggests insufficient supports/members " +
                    "— likely a mechanism. The stiffness factorization will confirm.");
            if (dsi == 0)
                return ValidationMessage.Info("Determinacy screen: frame is statically determinate (DSI = 0).");
            return ValidationMessage.Info(
                $"Determinacy screen: frame is statically indeterminate / redundant (DSI = {dsi}). This is stable, not an error.");
        }

        private static ValidationMessage DescribeTruss(int dsi)
        {
            if (dsi < 0)
                return ValidationMessage.Warning(
                    $"Determinacy screen: truss m + r - 2j = {dsi} (< 0) suggests a mechanism. " +
                    "The stiffness factorization will confirm.");
            if (dsi == 0)
                return ValidationMessage.Info("Determinacy screen: truss is statically determinate (m + r = 2j).");
            return ValidationMessage.Info(
                $"Determinacy screen: truss is statically indeterminate / redundant (m + r - 2j = {dsi}). This is stable, not an error.");
        }
    }
}
