using System.Collections.Generic;
using System.Linq;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Detects a global rigid-body translation mechanism: if no support anywhere
    /// restrains a global translation direction, the whole structure can slide in
    /// that direction (e.g. a portal frame on two rollers can sway horizontally).
    /// </summary>
    public class GlobalRestraintRule : IModelValidationRule
    {
        public IEnumerable<ValidationMessage> Validate(StructureModel model)
        {
            if (model.Elements.Count == 0)
                yield break;

            bool anyX = model.Supports.Any(s => s.RestrainsUx);
            bool anyY = model.Supports.Any(s => s.RestrainsUy);

            if (!anyX)
                yield return ValidationMessage.Error(
                    "No support restrains global X-translation: the structure has a horizontal " +
                    "rigid-body (sway) mechanism. Add a support that restrains Ux.");

            if (!anyY)
                yield return ValidationMessage.Error(
                    "No support restrains global Y-translation: the structure has a vertical " +
                    "rigid-body mechanism. Add a support that restrains Uy.");
        }
    }
}
