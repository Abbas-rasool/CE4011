using System;

namespace FrameAnalysisProgram.STRUCTURAL_MODEL
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

        public TemperatureLoad(
            FrameElement2D element,
            double uniformTemperatureChange,
            double temperatureGradient,
            double thermalExpansionCoeff,
            double memberDepth)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            UniformTemperatureChange = uniformTemperatureChange;
            TemperatureGradient = temperatureGradient;
            ThermalExpansionCoefficient = thermalExpansionCoeff;
            MemberDepth = memberDepth;
        }

        public void AssembleIntoVector(CustomVector globalLoadVector, DofMap dofMap, StructureModel model)
        {
            if (globalLoadVector == null)
                throw new ArgumentNullException(nameof(globalLoadVector));
            if (dofMap == null)
                throw new ArgumentNullException(nameof(dofMap));

            double length = Element.Length;

            // Axial force from uniform temperature change
            // N = E * A * α * T_u
            double EA = Element.Material.ElasticModulus * Element.Section.Area;
            double axialForce = EA * ThermalExpansionCoefficient * UniformTemperatureChange;

            // Moment from temperature gradient
            // M = E * I * α * ΔT / h
            // where h is member depth
            double EI = Element.Material.ElasticModulus * Element.Section.MomentOfInertia;
            double moment = EI * ThermalExpansionCoefficient * TemperatureGradient / MemberDepth;

            // Distribute axial force and moment to nodes
            // For symmetric loading, forces are split
            AssembleAtStartNode(globalLoadVector, dofMap, axialForce, moment);
            AssembleAtEndNode(globalLoadVector, dofMap, -axialForce, moment);
        }

        private void AssembleAtStartNode(
            CustomVector globalLoadVector,
            DofMap dofMap,
            double axialForce,
            double moment)
        {
            int eqX = dofMap.GetEquation(Element.StartNode.Id, 0);
            int eqRz = dofMap.GetEquation(Element.StartNode.Id, 2);

            if (eqX != 0)
                globalLoadVector.AddToEntry(eqX - 1, axialForce);
            if (eqRz != 0)
                globalLoadVector.AddToEntry(eqRz - 1, moment);
        }

        private void AssembleAtEndNode(
            CustomVector globalLoadVector,
            DofMap dofMap,
            double axialForce,
            double moment)
        {
            int eqX = dofMap.GetEquation(Element.EndNode.Id, 0);
            int eqRz = dofMap.GetEquation(Element.EndNode.Id, 2);

            if (eqX != 0)
                globalLoadVector.AddToEntry(eqX - 1, axialForce);
            if (eqRz != 0)
                globalLoadVector.AddToEntry(eqRz - 1, moment);
        }
    }
}
