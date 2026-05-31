using System;
using System.Collections.Generic;
using System.Text;
using MemberDesigner.DesignChecks;
using MemberDesigner.DesignChecks.American;
using MemberDesigner.DesignChecks.Eurocode;
using MemberDesigner.DesignChecks.Turkish;
using MemberDesigner.TimberDesignData.BaseClasses;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.Designers
{
    public class TimberDesignCheckProvider
    {

        #region Constructor
        public TimberDesignCheckProvider(TimberCheckTypeProvider checkTypeProvider)
        {
            // Later I will have to set it using the data i get from MD!
            _TimberCode = eTimberCode.EC5;

            _checks = new Dictionary<(eTimberDesignCheckType, eTimberCode), ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>>();
            _checkTypeProvider = checkTypeProvider;
        }
        #endregion

        #region Private Fields
        private Dictionary<(eTimberDesignCheckType, eTimberCode), ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>> _checks;
        TimberCheckTypeProvider _checkTypeProvider;
        private eTimberCode _TimberCode;

        #endregion

        #region Private Methods
        private ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData> GetNewCheck(eTimberDesignCheckType checkType)
        {

            if (_TimberCode == eTimberCode.US)
                return GetNewCheckAmerican(checkType);
            else if (_TimberCode == eTimberCode.TR)
                return GetNewCheckTS(checkType);
            else if (_TimberCode == eTimberCode.EC5)
                return GetNewCheckEC(checkType);
            else
                return null;
        }

        private ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData> GetNewCheckEC(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                case eTimberDesignCheckType.Parameters:
                    return new TimberParametersCheckEU();

                case eTimberDesignCheckType.Bending:
                    return new TimberDesCheckBendingEU();

                case eTimberDesignCheckType.Tension:
                    return new TimberDesCheckTensionEU();

                case eTimberDesignCheckType.Compression:
                    return new TimberDesCheckCompressionEU();

                case eTimberDesignCheckType.Shear:
                    return new TimberDesCheckShearEU();

                case eTimberDesignCheckType.CombinedBendingAxial:
                    return new TimberDesCheckCombinedEU();

                case eTimberDesignCheckType.SpacedColumn:
                    return null; // could be updated later.

                case eTimberDesignCheckType.BuiltUpBeam:
                    return null; // could be updated later.

                case eTimberDesignCheckType.BuiltUpColumn:
                    return null; // could be updated later.

                default:
                    return null;
            }

        }
        private ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData> GetNewCheckTS(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                case eTimberDesignCheckType.Bending:
                    return new TimberDesCheckBendingTS();

                case eTimberDesignCheckType.Tension:
                    return new TimberDesCheckTensionTS();

                case eTimberDesignCheckType.Compression:
                    return new TimberDesCheckCompressionTS();

                case eTimberDesignCheckType.Shear:
                    return new TimberDesCheckShearTS();

                case eTimberDesignCheckType.CombinedBendingAxial:
                    return new TimberDesCheckCombinedTS();

                case eTimberDesignCheckType.SpacedColumn:
                    return null; // could be updated later.

                case eTimberDesignCheckType.BuiltUpBeam:
                    return null; // could be updated later.

                case eTimberDesignCheckType.BuiltUpColumn:
                    return null; // could be updated later.

                default:
                    return null;
            }
        }
        private ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData> GetNewCheckAmerican(eTimberDesignCheckType checkType)
        {
            switch (checkType)
            {
                case eTimberDesignCheckType.Bending:
                    return new TimberDesCheckBendingUS();

                case eTimberDesignCheckType.Tension:
                    return new TimberDesCheckTensionUS();

                case eTimberDesignCheckType.Compression:
                    return new TimberDesCheckCompressionUS();

                case eTimberDesignCheckType.Shear:
                    return new TimberDesCheckShearUS();

                case eTimberDesignCheckType.CombinedBendingAxial:
                    return new TimberDesCheckCombinedBendingAxialUS();

                case eTimberDesignCheckType.SpacedColumn:
                    return null; // could be updated later.

                case eTimberDesignCheckType.BuiltUpBeam:
                    return null; // could be updated later.

                case eTimberDesignCheckType.BuiltUpColumn:
                    return null; // could be updated later.
                default:
                    return null;
            }
        }

        #endregion

        public List<ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>> ProvideChecks()
        {
            var requiredChecks = _checkTypeProvider.GetRequiredCheckTypes();

            ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData> check;

            var checkList = new List<ITimberDesignCheck<ITimberDesignCheckInput, TimberDesignCheckData>>();

            // This should be called from MD later!!
            var timberCode = eTimberCode.EC5;

            foreach (var checkType in requiredChecks)
            {
                if (!_checks.TryGetValue((checkType, timberCode), out check) || check == null)
                {
                    check = GetNewCheck(checkType);
                    _checks.Add((checkType, timberCode), check);
                }

                if (check != null) checkList.Add(check);
            }
            return checkList;
        }

    }
}
