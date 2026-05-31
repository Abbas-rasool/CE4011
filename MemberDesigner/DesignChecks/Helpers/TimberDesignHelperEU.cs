using System;
using System.Collections.Generic;
using System.Text;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Helpers
{
    public class TimberDesignHelperEU : TimberBaseDesignHelper
    {
        #region Constructor
        private TimberDesignHelperEU()
        {

        }
        #endregion

        #region Singleton Definition

        private static TimberDesignHelperEU? _instance;

        public static TimberDesignHelperEU GetInstance()
        {
            if (_instance == null) _instance = new TimberDesignHelperEU();

            return _instance;
        }

        public static void KillInstance()
        {
            _instance = null;
        }

        #endregion

        #region Public Methods
        public double CalculateVd(double lambdaEff, double maxCompressionDemand, double kcMajor)
        {
            double V_d;

            if (lambdaEff < 30)
            {
                V_d = maxCompressionDemand / (120 * kcMajor);
            }
            else if (lambdaEff >= 30 && lambdaEff < 60)
            {
                V_d = (lambdaEff * maxCompressionDemand) / (3600 * kcMajor);
            }
            else
            {
                V_d = maxCompressionDemand / (60 * kcMajor);
            }

            return V_d;
        }


        public double GetBetaC(eTimberMaterialType material)
        {
            switch (material)
            {
                case eTimberMaterialType.SolidTimber:
                    return 0.2;

                case eTimberMaterialType.GluedLaminatedTimber:
                case eTimberMaterialType.LVL:
                    return 0.1;

                default:
                    return 1.0;
            }
        }

        /// <summary>
        /// Returns the modification factor taking into account the effect of the duration of load and moisture content.
        /// </summary>
        public float GetK_ModFactor(eTimberMaterialType materialTypeEU, eServiceClass serviceClassEU, eLoadDurationClass loadDurationClassEU)
        {
            switch (materialTypeEU)
            {
                case eTimberMaterialType.SolidTimber:
                case eTimberMaterialType.GluedLaminatedTimber:
                case eTimberMaterialType.LVL:
                case eTimberMaterialType.Plywood:
                    return GetCommonTimberFactor(serviceClassEU, loadDurationClassEU);

                case eTimberMaterialType.OSB:
                    return GetOSBFactor(serviceClassEU, loadDurationClassEU);

                case eTimberMaterialType.ParticleBoards:
                    return GetParticleboardFactor(serviceClassEU, loadDurationClassEU);

                case eTimberMaterialType.FibreboardsHard:
                    return GetFibreboardHardFactor(serviceClassEU, loadDurationClassEU);

                case eTimberMaterialType.FibreboardsMedium:
                case eTimberMaterialType.FibreboardsMDF:
                    return GetFibreboardMediumOrMDF(loadDurationClassEU);

                default:
                    throw new ArgumentException("Unsupported material type.");
            }
        }

        #endregion

        #region Private Methods

        private float GetCommonTimberFactor(eServiceClass serviceClassEU, eLoadDurationClass loadDurationClassEU)
        {
            if (serviceClassEU == eServiceClass.ServiceClass1 || serviceClassEU == eServiceClass.ServiceClass2)
            {
                switch (loadDurationClassEU)
                {
                    case eLoadDurationClass.PermanentAction: return 0.60f;
                    case eLoadDurationClass.LongTermAction: return 0.70f;
                    case eLoadDurationClass.MediumTermAction: return 0.80f;
                    case eLoadDurationClass.ShortTermAction: return 0.90f;
                    case eLoadDurationClass.InstantaneousAction: return 1.10f;
                }
            }
            else if (serviceClassEU == eServiceClass.ServiceClass3)
            {
                switch (loadDurationClassEU)
                {
                    case eLoadDurationClass.PermanentAction: return 0.50f;
                    case eLoadDurationClass.LongTermAction: return 0.55f;
                    case eLoadDurationClass.MediumTermAction: return 0.65f;
                    case eLoadDurationClass.ShortTermAction: return 0.70f;
                    case eLoadDurationClass.InstantaneousAction: return 0.90f;
                }
            }

            throw new ArgumentException("Invalid service class or load duration class for timber.");
        }
        private float GetOSBFactor(eServiceClass serviceClassEU, eLoadDurationClass loadDurationClassEU)
        {
            if (serviceClassEU == eServiceClass.ServiceClass1)
            {
                switch (loadDurationClassEU)
                {
                    case eLoadDurationClass.PermanentAction: return 0.30f;
                    case eLoadDurationClass.LongTermAction: return 0.45f;
                    case eLoadDurationClass.MediumTermAction: return 0.65f;
                    case eLoadDurationClass.ShortTermAction: return 0.85f;
                    case eLoadDurationClass.InstantaneousAction: return 1.10f;
                }
            }
            else if (serviceClassEU == eServiceClass.ServiceClass2)
            {
                switch (loadDurationClassEU)
                {
                    case eLoadDurationClass.PermanentAction: return 0.30f;
                    case eLoadDurationClass.LongTermAction: return 0.40f;
                    case eLoadDurationClass.MediumTermAction: return 0.55f;
                    case eLoadDurationClass.ShortTermAction: return 0.70f;
                    case eLoadDurationClass.InstantaneousAction: return 0.90f;
                }
            }

            throw new ArgumentException("Invalid service class or load duration class for OSB.");
        }
        private float GetParticleboardFactor(eServiceClass sc, eLoadDurationClass ldc)
        {
            switch (sc)
            {
                case eServiceClass.ServiceClass1:
                    switch (ldc)
                    {
                        case eLoadDurationClass.PermanentAction: return 0.30f;
                        case eLoadDurationClass.LongTermAction: return 0.45f;
                        case eLoadDurationClass.MediumTermAction: return 0.65f;
                        case eLoadDurationClass.ShortTermAction: return 0.85f;
                        case eLoadDurationClass.InstantaneousAction: return 1.10f;
                    }
                    break;

                case eServiceClass.ServiceClass2:
                    switch (ldc)
                    {
                        case eLoadDurationClass.PermanentAction: return 0.20f;
                        case eLoadDurationClass.LongTermAction: return 0.30f;
                        case eLoadDurationClass.MediumTermAction: return 0.45f;
                        case eLoadDurationClass.ShortTermAction: return 0.60f;
                        case eLoadDurationClass.InstantaneousAction: return 0.80f;
                    }
                    break;
            }

            throw new ArgumentException("Invalid service class or load duration class for Particleboard.");
        }
        private float GetFibreboardHardFactor(eServiceClass sc, eLoadDurationClass ldc)
        {
            if (sc == eServiceClass.ServiceClass1)
            {
                switch (ldc)
                {
                    case eLoadDurationClass.PermanentAction: return 0.30f;
                    case eLoadDurationClass.LongTermAction: return 0.45f;
                    case eLoadDurationClass.MediumTermAction: return 0.65f;
                    case eLoadDurationClass.ShortTermAction: return 0.85f;
                    case eLoadDurationClass.InstantaneousAction: return 1.10f;
                }
            }
            else if (sc == eServiceClass.ServiceClass2)
            {
                switch (ldc)
                {
                    case eLoadDurationClass.PermanentAction: return 0.20f;
                    case eLoadDurationClass.LongTermAction: return 0.30f;
                    case eLoadDurationClass.MediumTermAction: return 0.45f;
                    case eLoadDurationClass.ShortTermAction: return 0.60f;
                    case eLoadDurationClass.InstantaneousAction: return 0.80f;
                }
            }

            throw new ArgumentException("Invalid service class or load duration class for Fibreboard Hard.");
        }
        private float GetFibreboardMediumOrMDF(eLoadDurationClass ldc)
        {
            switch (ldc)
            {
                case eLoadDurationClass.PermanentAction: return 0.20f;
                case eLoadDurationClass.LongTermAction: return 0.40f;
                case eLoadDurationClass.MediumTermAction: return 0.60f;
                case eLoadDurationClass.ShortTermAction: return 0.80f;
                case eLoadDurationClass.InstantaneousAction: return 1.10f;
                default:
                    throw new ArgumentException("Invalid load duration class for Fibreboard Medium or MDF.");
            }
        }


        #endregion
    }
}
