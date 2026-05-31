using System;
using System.Collections.Generic;
using System.Text;

namespace MemberDesigner.TimberDesignData.BaseClasses
{
    /// <summary>
    /// Timber design check data according to American code (e.g., NDS).
    /// </summary>
    public abstract class TimberDesignCheckDataAmerican : TimberDesignCheckData
    {
        #region USA-Specific Properties

        public float LoadDurationFactor { get; set; }
        public float WetServiceFactor { get; set; }
        public float TemperatureFactor { get; set; }
        public float SizeFactor { get; set; }
        public float IncisingFactor { get; set; }
        public float FormatConversionFactor { get; set; }
        public float ResistanceFactor { get; set; }
        public float TimeEffectFactor { get; set; }
        public float VolumeFactor { get; set; }
        public float FormatConversionFactorEmin { get; set; }
        public float ResistanceFactorEmin { get; set; }

        #endregion
    }
}
