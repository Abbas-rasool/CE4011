using System;
using System.Collections.Generic;
using System.Linq;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Raised when a model cannot be analyzed (e.g. a mechanism, disconnected
    /// model, or a non-positive-definite stiffness matrix). Carries the clear
    /// validation messages explaining the likely cause so callers can present
    /// them instead of a raw stack trace.
    /// </summary>
    public class StructuralAnalysisException : Exception
    {
        public IReadOnlyList<ValidationMessage> Messages { get; }

        public StructuralAnalysisException(IReadOnlyList<ValidationMessage> messages)
            : base(BuildMessage(messages))
        {
            Messages = messages ?? new List<ValidationMessage>();
        }

        public StructuralAnalysisException(ValidationMessage message)
            : this(new List<ValidationMessage> { message })
        {
        }

        private static string BuildMessage(IReadOnlyList<ValidationMessage> messages)
        {
            if (messages == null || messages.Count == 0)
                return "The structure could not be analyzed.";

            return "The structure could not be analyzed:" + Environment.NewLine +
                   string.Join(Environment.NewLine, messages.Select(m => "  - " + m.Message));
        }
    }
}
