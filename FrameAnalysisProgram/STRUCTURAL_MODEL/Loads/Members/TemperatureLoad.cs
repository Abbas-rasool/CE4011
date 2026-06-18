using System;
using FrameAnalysisProgram.ANALYSIS_CORE;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Elements;
using FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Interfaces;
using Matrix_Library.MAIN_TYPES;
using StructuralLoads;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL.Loads.Members
{
    /// <summary>
    /// Represents a temperature load on a member.
    /// 
    /// Temperature effects:
    /// - T_u = uniform temperature change causes axial strain and axial force
    /// - T_g = temperature gradient (top - bottom) causes bending
    /// 
    /// Requires material thermal expansion coefficient.
    /// </summary>
    public class TemperatureLoad : IMemberLoad
    {
        /// <summary>
        /// The member (element) where the temperature load is applied.
        /// </summary>
        public FrameElement2D Element { get; }

        /// <summary>
        /// Uniform temperature change across the section.
        /// Units: temperature (e.g., degrees Celsius)
        /// </summary>
        public double UniformTemperatureChange { get; }

        /// <summary>
        /// Temperature gradient (top surface temperature - bottom surface temperature).
        /// Units: temperature
        /// </summary>
        public double TemperatureGradient { get; }

        /// <summary>
        /// Coefficient of thermal expansion.
        /// Units: 1/temperature
        /// </summary>
        public double ThermalExpansionCoefficient { get; }

        /// <summary>
        /// Depth of the member (used for gradient calculations).
        /// Units: length
        /// </summary>
        public double MemberDepth { get; }

        /// <summary>
        /// A temperature load is always a self-straining thermal action.
        /// </summary>
        public eLoadNature Nature => eLoadNature.Thermal;

        public TemperatureLoad(FrameElement2D element, double uniformTemperatureChange, double temperatureGradient, double thermalExpansionCoeff, double memberDepth)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));

            UniformTemperatureChange = uniformTemperatureChange;
            TemperatureGradient = temperatureGradient;
            ThermalExpansionCoefficient = thermalExpansionCoeff;

            if (memberDepth <= 0.0)
                throw new ArgumentException("Member depth must be positive.", nameof(memberDepth));

            MemberDepth = memberDepth;
        }

        public void AssembleIntoVector(CustomVector globalLoadVector, DofMap dofMap, StructureModel model)
        {
            if (globalLoadVector == null)
                throw new ArgumentNullException(nameof(globalLoadVector));
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            double[] equivalentLoads = Element.GetEquivalentGlobalNodalLoads(GetLocalFixedEndForces());
            int[] dofIndices = Element.GetGlobalDofIndices(dofMap);

            for (int i = 0; i < 6; i++)
            {
                int eq = dofIndices[i];
                if (eq != 0)
                    globalLoadVector.AddToEntry(eq - 1, equivalentLoads[i]);
            }
        }

        public double[] GetLocalFixedEndForces()
        {
            double E = Element.Material.ElasticModulus;
            double A = Element.Section.Area;
            double I = Element.Section.MomentOfInertia;

            // Restrained axial force from uniform temperature change: N = E A α T_u.
            double axialForce = E * A * ThermalExpansionCoefficient * UniformTemperatureChange;

            // Restrained end moment from temperature gradient: M = E I α ΔT / h,
            // applied as equal and opposite fixed-end moments (constant curvature).
            double moment = E * I * ThermalExpansionCoefficient * TemperatureGradient / MemberDepth;

            double[] f = new double[6];
            f[0] = axialForce;
            f[3] = -axialForce;
            f[2] = moment;
            f[5] = -moment;

            return f;
        }
    }
}
