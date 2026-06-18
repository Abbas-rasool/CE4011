using System.Collections.Generic;
using System.Linq;
using FrameAnalysisProgram.STRUCTURAL_MODEL;

namespace FrameAnalysisProgram.ANALYSIS_CORE.Validation
{
    /// <summary>
    /// Runs a configurable set of model validation rules and aggregates their
    /// findings. Rules are injected (Dependency Inversion), so the set of checks
    /// can be extended or customized without changing this class.
    /// </summary>
    public class ModelValidator
    {
        private readonly IReadOnlyList<IModelValidationRule> _rules;

        public ModelValidator(IEnumerable<IModelValidationRule> rules)
        {
            _rules = rules?.ToList() ?? new List<IModelValidationRule>();
        }

        /// <summary>
        /// The standard rule set covering the common modelling errors.
        /// </summary>
        public static ModelValidator CreateDefault()
        {
            return new ModelValidator(new IModelValidationRule[]
            {
                new OrphanNodeRule(),
                new ConnectivityRule(),
                new GlobalRestraintRule(),
                new DeterminacyRule()
            });
        }

        public ModelValidationResult Validate(StructureModel model)
        {
            var messages = new List<ValidationMessage>();

            foreach (var rule in _rules)
                messages.AddRange(rule.Validate(model));

            return new ModelValidationResult(messages);
        }
    }
}
