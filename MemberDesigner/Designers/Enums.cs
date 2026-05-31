using System;
using System.Collections.Generic;
using System.Text;

namespace MemberDesigner.Designers
{
    public static class Enums
    {
        #region Common Enums
        public enum eTimberCode
        {
            US,
            EC5,
            TR
        }

        public enum eDesignStatus : byte
        {
            Pass = 0,
            Fail = 1,
            Warning = 2
        }

        public enum eTimberDesignCheckType : byte
        {
            Compression = 0,
            Tension = 1,
            Bending = 2,
            Shear = 3,
            Serviceability = 4,
            CombinedBendingAxial = 5,
            SpacedColumn = 6,
            BuiltUpBeam = 7,
            BuiltUpColumn = 8,
            ShearWall = 9,
            Parameters = 10
        }

        public enum eBuiltUPColumnConnectionType : byte
        {
            None = 0,
            Nailed = 1,
            Bolted = 2,
            Glued = 3
        }

        public enum eTimberMaterialType : byte
        {
            SolidTimber = 0,
            GluedLaminatedTimber = 1,
            LVL = 2,
            Plywood = 3,
            OSB = 4,
            ParticleBoards = 5,
            FibreboardsHard = 6,
            FibreboardsMedium = 7,
            FibreboardsMDF = 8,
            FibreboardsSoft = 9,
            Connections = 10,
            PunchedMetalPlateFasteners = 11,
            AccidentalCombinations = 12,
            /// <summary>
            /// added for the turkish standards.
            /// </summary>
            CLT = 13
        }

        public enum eServiceClass : byte
        {
            /// <summary>
            /// for turkish standards (az)
            /// </summary>
            ServiceClass1 = 0,

            /// <summary>
            /// for turkish standards (orta)
            /// </summary>
            ServiceClass2 = 1,

            /// <summary>
            /// for turkish standards (cok)
            /// </summary>
            ServiceClass3 = 2
        }

        /// <summary>
        /// Note: For turkish standards, there's only Long-Term, Medium-Term and Instantaneous actions.
        /// </summary>
        public enum eLoadDurationClass : byte
        {
            PermanentAction = 0,
            LongTermAction = 1,
            MediumTermAction = 2,
            ShortTermAction = 3,
            InstantaneousAction = 4
        }
        #endregion

        #region American Code
        /// <summary>
        /// Shear Wall Enums
        /// </summary>
        public enum eLoadingCombinationType
        {
            ASD = 1,
            LRFD = 2
        }

        public enum eSheathingMatType
        {
            /// <summary>
            /// Wood structural panels
            /// </summary>
            WSP = 0,

            /// <summary>
            /// ParticleBoard
            /// </summary>
            Prb = 1,

            /// <summary>
            /// Diagonally-sheathed lumber
            /// </summary>
            Dsl = 2,

            /// <summary>
            /// Gypsum wallboard
            /// </summary>
            Gw = 3,

            /// <summary>
            /// Portland cement plaster
            /// </summary>
            Pcp = 4,

            /// <summary>
            /// Structural Fiberboard
            /// </summary>
            Sf = 5
        }

        public enum eblockingType
        {
            Blocked = 0,
            Unblocked = 1
        }

        public enum eShearWallAnalysisType
        {
            /// <summary>
            /// Full-Height Wall Segments
            /// </summary>
            FHS = 0,

            /// <summary>
            /// Force-Transfer Around Openings
            /// </summary>
            FTAO = 1,

            /// <summary>
            /// Perforated Shear Walls
            /// </summary>
            PSW = 2
        }

        public enum eNailLabels
        {
            d6 = 0,
            d8 = 1,
            d10 = 2
        }

        /// <summary>
        /// NDS Table 2.3.2 - Frequently Used Load Durations (C_D)
        /// </summary>
        public enum eLoadDuration : byte
        {
            /// <summary>
            /// Dead Load
            /// </summary>
            Permanent = 0,
            /// <summary>
            /// Occupancy Live Load
            /// </summary>
            TenYears = 1,
            /// <summary>
            /// Snow Load
            /// </summary>
            TwoMonths = 2,
            /// <summary>
            /// Construction Load
            /// </summary>
            SevenDays = 3,
            /// <summary>
            /// Wind/EQ Load
            /// </summary>
            TenMinutes = 4,
            /// <summary>
            /// Impact Load
            /// </summary>
            Impact = 5
        }

        public enum eShaftConnectionType : byte
        {
            /// <summary>
            /// wedged (TS)
            /// </summary>
            Pack = 0,
            /// <summary>
            /// battened (TS)
            /// </summary>
            Gusset = 1
        }

        /// <summary>
        /// The service condition is based on the moisture content, which is given as a percentage for each product type separately.
        /// </summary>
        public enum eMoistureContentCondition : byte
        {
            /// <summary>
            /// dry service conditions
            /// </summary>
            DryServiceConditon = 0,

            /// <summary>
            /// wet service conditions
            /// </summary>
            WetServiceCondition = 1
        }

        /// <summary>
        /// Different Design Parameters used in the calculations.
        /// </summary>
        public enum eDesignParameter : byte
        {
            /// <summary>
            /// Fb: bending
            /// </summary>
            Fb = 0,

            /// <summary>
            /// Ft: tensile
            /// </summary>
            Ft = 1,

            /// <summary>
            /// Fv: shear
            /// </summary>
            Fv = 2,

            /// <summary>
            /// Fc90: compression perpendicular to the fibers
            /// </summary>
            Fc90 = 3,

            /// <summary>
            /// Fc: compression parallel to the fibers
            /// </summary>
            Fc = 4,

            /// <summary>
            /// E: Modulus of Elasticity
            /// </summary>
            E = 5,

            /// <summary>
            /// Emin: Adjusted Modulus of Elasticity
            /// </summary>
            Emin = 6,

            /// <summary>
            /// Fs: rollig shear
            /// </summary>
            Fs = 7,

            /// <summary>
            /// Frt: reference radial tension design value perpendicular to grain for structural glued laminated timber. 
            /// </summary>
            Rrt = 8
        }

        /// <summary>
        /// Timber types adressed in the NDS chapters
        /// </summary>
        public enum eTimberType : byte
        {
            /// <summary>
            /// SL = Sawn Lumber
            /// </summary>
            SL = 0,

            /// <summary>
            /// SGLT = Structural Glued Laminated Timber
            /// </summary>
            SGLT = 1,

            /// <summary>
            /// RTPP = Round Timber Poles and Piles
            /// </summary>
            RTPP = 2,

            /// <summary>
            /// PWJ = Prefabricated Wood I-Joists
            /// </summary>
            PWJ = 3,

            /// <summary>
            /// SCL = Structural Composite Lumber
            /// </summary>
            SCL = 4,

            /// <summary>
            /// WSP = Wood Structural Panels
            /// </summary>
            WSP = 5,

            /// <summary>
            /// CLT = Cross Laminated Timber
            /// </summary>
            CLT = 6
        }

        public enum eApplicationType : byte
        {
            /// <summary>
            /// member design
            /// </summary>
            Member = 1,

            /// <summary>
            /// all connections
            /// </summary>
            Connection = 2,

            /// <summary>
            /// deck
            /// </summary>
            Deck = 3
        }

        /// <summary>
        /// Timber grades following NDS Supplement for Visually Graded Dimension Lumber.
        /// </summary>
        public enum eTimberGrades : byte
        {
            /// <summary>
            /// Select Structural grade for high-quality timber used in structural applications.
            /// </summary>
            SS = 0,

            /// <summary>
            /// No. 1 grade timber, suitable for most structural applications with minimal defects.
            /// </summary>
            No1 = 1,

            /// <summary>
            /// No. 2 grade timber, suitable for general construction purposes.
            /// </summary>
            No2 = 2,

            /// <summary>
            /// No. 3 grade timber, used for less critical applications with more defects.
            /// </summary>
            No3 = 3,

            /// <summary>
            /// Stud grade timber, typically used in framing and non-load-bearing structures.
            /// </summary>
            Stud = 4,

            /// <summary>
            /// Construction grade timber, used for general construction purposes but with minor defects.
            /// </summary>
            Cons = 5,

            /// <summary>
            /// Standard grade timber, suitable for moderate strength applications.
            /// </summary>
            Stan = 6,

            /// <summary>
            /// Utility grade timber, primarily for non-structural uses or temporary applications.
            /// </summary>
            Ut = 7
        }

        public enum eLoadCombinationType : byte
        {
            LRFD = 0,
            ASD = 1
        }

        public enum eTimberMemberDesignArgument : byte
        {
            ReduceSlendernessRatio = 0,
            IncreaseCrossSectionArea = 1,
            IncreaseMaterialStrength = 2,
            IncreaseMemberDepth = 3,
            IncreaseMemberThickness = 4,
            ReduceMemberLength = 5,

            /// <summary>
            /// bearing shall be on a metal plate or strap, or on other equivalently durable, rigid, homogeneous materail with sufficient stiffness to distribute the applied load.
            /// </summary>
            AddMetalPlateOrStrap = 6,

            GeometricConstraintsShouldBeRechecked = 7
        }

        public enum eMemberConfigurationType : byte
        {
            SolidMembers = 0,
            NotSpacedCombinedColumns = 1,
            SpacedColumns = 2
        }

        public enum eSpeciesGroup : byte
        {
            /// <summary>
            /// Strongest species
            /// </summary>
            A = 0,

            /// <summary>
            /// Medium-strong species
            /// </summary>
            B = 1,

            /// <summary>
            /// Moderate species
            /// </summary>
            C = 2,

            /// <summary>
            /// Weaker species
            /// </summary>
            D = 3
        }

        public enum eWoodMaterialType : byte
        {
            Softwood = 0,
            Hardwood = 1
        }
        #endregion

        #region EU 
        public enum eSupportTypeEU : byte
        {
            Continuous = 0,
            Discrete = 2
        }
        #endregion
    }
}

