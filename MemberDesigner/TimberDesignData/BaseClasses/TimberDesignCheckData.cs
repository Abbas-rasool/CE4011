using System;
using System.Collections.Generic;
using System.Text;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberDesignData.BaseClasses
{
    /// <summary>
    /// Abstract base class for all timber design check outputs.
    /// Provides common properties and abstract methods for design check results.
    /// </summary>
    public abstract class TimberDesignCheckData
    {
        #region Private Fields

        private eDesignStatus _designStatus; // Using the enum directly
        private readonly List<eTimberMemberDesignArgument> _designArguments = new();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the overall design status of the check (e.g., Passed, Failed, Warning).
        /// Marked with ProtoMember for serialization.
        /// </summary>
        public eDesignStatus DesignStatus
        {
            get => _designStatus;
            set => _designStatus = value;
        }

        /// <summary>
        /// Gives recommendations to user on how to improve the design and avoid failure.
        /// </summary>
        public List<eTimberMemberDesignArgument> DesignArguments
        {
            get => _designArguments;
        }

        /// <summary>
        /// Gets the specific type of timber design check represented by this output.
        /// Each concrete output class must define this.
        /// </summary>
        public abstract eTimberDesignCheckType CheckType { get; }

        /// <summary>
        /// Gets the localized title of the design check
        /// </summary>
        public abstract string GetTitle();

        /// <summary>
        /// Generates an HTML styled string on the design summary. Should be used in UI
        /// </summary>
        public abstract string GetSummary();

        /// <summary>
        /// Gets a report section with detailed check result
        /// </summary>
        public abstract string GetDetailedReportSection();

        /// <summary>
        /// Gets the utilization ratio in decimal format
        /// </summary>
        public abstract double GetUtilizationRatio();

        #endregion
    }
}
