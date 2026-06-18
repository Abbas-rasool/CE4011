using System.Collections.Generic;
using System.Linq;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Aggregated result of running the model validation rules.
    /// </summary>
    public class ModelValidationResult
    {
        public IReadOnlyList<ValidationMessage> Messages { get; }

        public ModelValidationResult(IReadOnlyList<ValidationMessage> messages)
        {
            Messages = messages ?? new List<ValidationMessage>();
        }

        public bool HasErrors => Messages.Any(m => m.Severity == ValidationSeverity.Error);

        public bool HasWarnings => Messages.Any(m => m.Severity == ValidationSeverity.Warning);

        public IEnumerable<ValidationMessage> Errors =>
            Messages.Where(m => m.Severity == ValidationSeverity.Error);
    }
}
