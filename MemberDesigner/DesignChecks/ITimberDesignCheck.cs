using System.Text;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks
{
    /// <summary>
    /// Base interface for all timber design check inputs.
    /// </summary>
    public interface ITimberDesignCheckInput
    {
        eTimberDesignCheckType CheckType { get; }
    }
}


