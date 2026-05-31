using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks
{
    /// <summary>
    /// Generic interface for a timber design check.
    /// Note: Contravariance and Covariance is used for this interface, confirm it doesn't make a problem!!
    /// </summary>
    /// <typeparam name="TInput">The specific input type for this check, must implement ITimberDesignCheckInput.</typeparam>
    /// <typeparam name="TOutput">The specific output type for this check, must implement ITimberDesignCheckOutput.</typeparam>
    public interface ITimberDesignCheck<in TInput, out TOutput>
        where TInput : ITimberDesignCheckInput
        where TOutput : TimberDesignCheckData
    {

        /// <summary>
        /// Checks the design of member against a certain code requirement.
        /// </summary>
        /// <param name="input">Input parameters for the check</param>
        /// <param name="dependencies">Results from other checks on which the current check depends on</param>
        /// <returns>A output data class of the specified type</returns>
        TOutput PerformCheck(TInput input, params TimberDesignCheckData[] dependencies);
        eTimberDesignCheckType CheckType { get; }

        /// <summary>
        /// This is a list that specifies other design checks, the result of which, this check depends on
        /// </summary>
        List<eTimberDesignCheckType> Dependencies { get; }
    }
}
