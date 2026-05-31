using System;
using System.Collections.Generic;
using System.Text;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.Designers
{
    public class TimberCheckTypeProvider
    {
        public List<eTimberDesignCheckType> GetRequiredCheckTypes()
        {
            // Initialize the list of required check types
            List<eTimberDesignCheckType> checkTypes = new List<eTimberDesignCheckType>();

            // Add general required check types
            checkTypes.AddRange(
                new List<eTimberDesignCheckType>
                {
                    eTimberDesignCheckType.Parameters,
                    eTimberDesignCheckType.Tension,
                    eTimberDesignCheckType.Compression,
                    eTimberDesignCheckType.Shear,
                    eTimberDesignCheckType.Bending,
                    eTimberDesignCheckType.CombinedBendingAxial,

                });

            return checkTypes;
        }

    }
}
