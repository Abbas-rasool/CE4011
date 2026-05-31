using System;
using System.Collections.Generic;
using System.Text;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.DesignChecks.Helpers
{
    public class TimberDesignHelperTS : TimberBaseDesignHelper
    {
        #region Constructor
        private TimberDesignHelperTS()
        {

        }
        #endregion

        #region Singleton Definition

        private static TimberDesignHelperTS? _instance;

        public static TimberDesignHelperTS GetInstance()
        {
            if (_instance == null) _instance = new TimberDesignHelperTS();

            return _instance;
        }

        public static void KillInstance()
        {
            _instance = null;
        }

        #endregion

        #region Public Methods

        public double GetMaterialFactor(eTimberMaterialType material)
        {
            switch (material)
            {
                case eTimberMaterialType.SolidTimber:
                    return 0.8;

                case eTimberMaterialType.GluedLaminatedTimber:
                    return 0.9;

                default:
                    return 1.0;
            }
        }


        /// <summary>
        /// Calculates the buckling coefficient (C_p) according to Eq. (4.23).
        /// Formula:
        /// C_p = ( 1 + (f_E / (f_c0k))/ (2 * c) - sqrt( (1 + (f_E / (2 * c * f_c0k)))^2 - (f_E / (c * f_c0k)) ) )
        /// </summary>
        public double CalculateC_P(double f_E, double f_c0k, double c)
        {
            double term1 = (1 + (f_E / (f_c0k))) / 2 * c;
            double insideSqrt = Math.Pow(term1, 2) - (f_E / (c * f_c0k));
            double result = (term1 - Math.Sqrt(insideSqrt));

            return result;
        }

        /// <summary>
        /// Calculates the critical buckling design stress (F_E) using Euler's formula.
        /// Formula: F_E = (π² * E) / (λ²)
        /// </summary>
        /// <param name="slendernessRatio">The slenderness ratio (λ) of the member.</param>
        /// <param name="E_005">The effective elastic modulus (E, typically at 0.05 percentile).</param>
        /// <returns>The critical buckling design stress (F_E).</returns>
        public double Calculatef_E(double slendernessRatio, double E_005)
        {
            double piSquared = Math.PI * Math.PI;
            return (piSquared * E_005) / Math.Pow(slendernessRatio, 2);
        }

        /// <summary>
        /// Returns the humidity modification factor (C_N)
        /// Tablo 1.4 Nem Durumu Duzeltme Katsayisi.
        /// </summary>
        public float GetC_N(eServiceClass serviceClass)
        {
            switch (serviceClass)
            {
                case eServiceClass.ServiceClass1:
                    return 1.0f;
                case eServiceClass.ServiceClass2:
                    return 0.95f;
                case eServiceClass.ServiceClass3:
                    return 0.85f;
                default:
                    return 0.85f;
            }
        }

        /// <summary>
        /// Returns the modification factor taking into account the effect of the duration of load and moisture content.
        /// </summary>
        public float GetC_Y(eTimberMaterialType materialType, eServiceClass serviceClass, eLoadDurationClass loadDurationClass)
        {
            switch (materialType)
            {
                case eTimberMaterialType.SolidTimber:
                case eTimberMaterialType.GluedLaminatedTimber:
                case eTimberMaterialType.CLT:
                case eTimberMaterialType.LVL:
                case eTimberMaterialType.Plywood:
                    return GetCommonTimberFactor(loadDurationClass);

                case eTimberMaterialType.OSB:
                    return GetOSBFactor(serviceClass, loadDurationClass);

                case eTimberMaterialType.ParticleBoards:
                    return GetParticleboardFactor(serviceClass, loadDurationClass);

                case eTimberMaterialType.FibreboardsHard:
                    return GetFibreboardHardFactor(serviceClass, loadDurationClass);

                case eTimberMaterialType.FibreboardsMedium:
                case eTimberMaterialType.FibreboardsMDF:
                    return GetFibreboardMediumOrMDF(serviceClass, loadDurationClass);

                default:
                    throw new ArgumentException("Unsupported material type.");
            }
        }

        /// <summary>
        ///  Calculates the net area for tension members considering it's connection.
        /// </summary>
        public static double CalculateEffectiveNetArea(
            double grossArea,
            double holeCount,
            double elementThickness,
            double boltDiameter,
            double splitRingThickness = 0,
            double splitRingDiameter = 0)
        {

            double holeDiameter = boltDiameter + 1.0;

            // 2. The formula for effective net area is:
            // An = Ag - (sum of dh * (t - 2a)) - (sum of dbb * a)

            double subtractedArea1 = holeCount * holeDiameter * (elementThickness - (2 * splitRingThickness));
            double subtractedArea2 = holeCount * splitRingDiameter * splitRingThickness;

            double netArea = grossArea - subtractedArea1 - subtractedArea2;

            if (boltDiameter >= 5.0)
            {
                double limitArea = 0.8 * grossArea;
                netArea = Math.Min(netArea, limitArea);
            }

            return netArea;
        }

        #endregion

        #region Private Methods

        private float GetCommonTimberFactor(eLoadDurationClass loadDurationClass)
        {
            switch (loadDurationClass)
            {
                case eLoadDurationClass.PermanentAction: return 0.60f;
                case eLoadDurationClass.MediumTermAction: return 0.80f;
                case eLoadDurationClass.InstantaneousAction: return 1.10f;
                default:
                    return 1.0f;
            }

        }

        private float GetOSBFactor(eServiceClass serviceClass, eLoadDurationClass loadDurationClass)
        {
            if (serviceClass == eServiceClass.ServiceClass1)
            {
                switch (loadDurationClass)
                {
                    case eLoadDurationClass.PermanentAction: return 0.30f;
                    case eLoadDurationClass.MediumTermAction: return 0.60f;
                    case eLoadDurationClass.InstantaneousAction: return 1.10f;
                }
            }
            else if (serviceClass == eServiceClass.ServiceClass2)
            {
                switch (loadDurationClass)
                {
                    case eLoadDurationClass.PermanentAction: return 0.30f;
                    case eLoadDurationClass.MediumTermAction: return 0.60f;
                    case eLoadDurationClass.InstantaneousAction: return 0.95f;
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
                        case eLoadDurationClass.MediumTermAction: return 0.65f;
                        case eLoadDurationClass.InstantaneousAction: return 1.10f;
                    }
                    break;

                case eServiceClass.ServiceClass2:
                    switch (ldc)
                    {
                        case eLoadDurationClass.PermanentAction: return 0.20f;
                        case eLoadDurationClass.MediumTermAction: return 0.45f;
                        case eLoadDurationClass.InstantaneousAction: return 0.85f;
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
                    case eLoadDurationClass.MediumTermAction: return 0.65f;
                    case eLoadDurationClass.InstantaneousAction: return 1.10f;
                }
            }
            else if (sc == eServiceClass.ServiceClass2)
            {
                switch (ldc)
                {
                    case eLoadDurationClass.PermanentAction: return 0.20f;
                    case eLoadDurationClass.MediumTermAction: return 0.45f;
                    case eLoadDurationClass.InstantaneousAction: return 0.85f;
                }
            }

            throw new ArgumentException("Invalid service class or load duration class for Fibreboard Hard.");
        }

        private float GetFibreboardMediumOrMDF(eServiceClass sc, eLoadDurationClass ldc)
        {
            if (sc == eServiceClass.ServiceClass1)
            {
                switch (ldc)
                {
                    case eLoadDurationClass.PermanentAction: return 0.20f;
                    case eLoadDurationClass.MediumTermAction: return 0.60f;
                    case eLoadDurationClass.InstantaneousAction: return 1.10f;
                }
            }
            else if (sc == eServiceClass.ServiceClass2)
            {
                switch (ldc)
                {
                    case eLoadDurationClass.PermanentAction: return 0.00f;
                    case eLoadDurationClass.MediumTermAction: return 0.00f;
                    case eLoadDurationClass.InstantaneousAction: return 0.85f;
                }
            }

            throw new ArgumentException("Invalid load duration class for Fibreboard Medium or MDF.");
        }

        #endregion
    }
}
