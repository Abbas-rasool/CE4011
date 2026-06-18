namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Severity of a model validation finding.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>Informational only — analysis proceeds (e.g. statically indeterminate).</summary>
        Info,

        /// <summary>Suspicious — analysis proceeds but results may be unreliable.</summary>
        Warning,

        /// <summary>Fatal — the model cannot be analyzed as given.</summary>
        Error
    }

    /// <summary>
    /// A single, human-readable validation finding. The message text is expected
    /// to be self-explanatory and to name the location (node / DOF / direction)
    /// of the problem where applicable.
    /// </summary>
    public class ValidationMessage
    {
        public ValidationSeverity Severity { get; }
        public string Message { get; }

        public ValidationMessage(ValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message ?? string.Empty;
        }

        public static ValidationMessage Info(string message) => new ValidationMessage(ValidationSeverity.Info, message);
        public static ValidationMessage Warning(string message) => new ValidationMessage(ValidationSeverity.Warning, message);
        public static ValidationMessage Error(string message) => new ValidationMessage(ValidationSeverity.Error, message);

        public override string ToString() => $"[{Severity}] {Message}";
    }
}
