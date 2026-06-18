using System.Collections.Generic;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// A single model-level validation rule (checked before assembly/solve).
    /// Each rule is responsible for one class of problem and returns zero or more
    /// findings. New rules can be added without modifying existing ones.
    /// </summary>
    public interface IModelValidationRule
    {
        IEnumerable<ValidationMessage> Validate(StructureModel model);
    }
}
