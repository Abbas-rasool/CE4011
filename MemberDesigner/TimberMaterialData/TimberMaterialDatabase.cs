using System.Collections.Generic;
using static MemberDesigner.Designers.Enums;

namespace MemberDesigner.TimberMaterialData
{
    /// <summary>
    /// Static lookup of timber material design values, one facade over two families:
    /// EN 338 / EN 14080 characteristic values (used by EC5 and TR) keyed by
    /// <see cref="eStrengthClass"/>, and NDS reference design values (used by US) keyed by
    /// <see cref="eNdsSpeciesGrade"/>. All strengths/moduli are returned in MPa.
    /// </summary>
    public static class TimberMaterialDatabase
    {
        /// <summary>psi → MPa (N/mm²) conversion factor.</summary>
        public const float PsiToMpa = 0.00689476f;

        // --- EN 338 (C/D) and EN 14080 (GL), values in MPa / (kg/m³) ---
        // Per EN 338:2016 and EN 14080. Representative; verify against the governing edition.
        private static readonly Dictionary<eStrengthClass, EnStrengthProperties> _en = new()
        {
            //                              Fmk  Ft0k  Ft90 Fc0k Fc90 Fvk  E0mean E005  RhoK
            [eStrengthClass.C14] = new(14f,  7.2f, 0.4f, 16f, 2.0f, 3.0f,  7000f,  4700f, 290f),
            [eStrengthClass.C16] = new(16f,  8.5f, 0.4f, 17f, 2.2f, 3.2f,  8000f,  5400f, 310f),
            [eStrengthClass.C18] = new(18f, 10.0f, 0.4f, 18f, 2.2f, 3.4f,  9000f,  6000f, 320f),
            [eStrengthClass.C20] = new(20f, 11.5f, 0.4f, 19f, 2.3f, 3.6f,  9500f,  6400f, 330f),
            [eStrengthClass.C22] = new(22f, 13.0f, 0.4f, 20f, 2.4f, 3.8f, 10000f,  6700f, 340f),
            [eStrengthClass.C24] = new(24f, 14.0f, 0.4f, 21f, 2.5f, 4.0f, 11000f,  7400f, 350f),
            [eStrengthClass.C27] = new(27f, 16.5f, 0.4f, 22f, 2.6f, 4.0f, 11500f,  7700f, 370f),
            [eStrengthClass.C30] = new(30f, 19.0f, 0.4f, 24f, 2.7f, 4.0f, 12000f,  8000f, 380f),
            [eStrengthClass.C35] = new(35f, 22.5f, 0.4f, 25f, 2.8f, 4.0f, 13000f,  8700f, 400f),
            [eStrengthClass.C40] = new(40f, 26.0f, 0.4f, 27f, 2.9f, 4.0f, 14000f,  9400f, 420f),
            [eStrengthClass.C45] = new(45f, 30.0f, 0.4f, 29f, 3.1f, 4.0f, 15000f, 10000f, 440f),
            [eStrengthClass.C50] = new(50f, 33.5f, 0.4f, 30f, 3.2f, 4.0f, 16000f, 10700f, 460f),

            [eStrengthClass.D18] = new(18f, 11.0f, 0.6f, 18f, 7.5f, 3.5f,  9500f,  8000f, 475f),
            [eStrengthClass.D24] = new(24f, 14.0f, 0.6f, 21f, 7.8f, 4.0f, 10000f,  8500f, 485f),
            [eStrengthClass.D30] = new(30f, 18.0f, 0.6f, 23f, 8.0f, 4.0f, 11000f,  9200f, 530f),
            [eStrengthClass.D35] = new(35f, 21.0f, 0.6f, 25f, 8.4f, 4.0f, 12000f, 10100f, 540f),
            [eStrengthClass.D40] = new(40f, 24.0f, 0.6f, 26f, 8.8f, 4.0f, 13000f, 10900f, 550f),
            [eStrengthClass.D50] = new(50f, 30.0f, 0.6f, 29f, 9.7f, 4.0f, 14000f, 11800f, 620f),
            [eStrengthClass.D60] = new(60f, 36.0f, 0.6f, 32f,10.5f, 4.5f, 17000f, 14300f, 700f),
            [eStrengthClass.D70] = new(70f, 42.0f, 0.6f, 34f,13.5f, 5.0f, 20000f, 16800f, 900f),

            [eStrengthClass.GL20h] = new(20f, 16.0f, 0.5f, 20.0f, 2.5f, 3.5f,  8400f,  7000f, 340f),
            [eStrengthClass.GL22h] = new(22f, 17.6f, 0.5f, 22.0f, 2.5f, 3.5f, 10500f,  8800f, 370f),
            [eStrengthClass.GL24h] = new(24f, 19.2f, 0.5f, 24.0f, 2.5f, 3.5f, 11500f,  9600f, 385f),
            [eStrengthClass.GL26h] = new(26f, 20.8f, 0.5f, 26.0f, 2.5f, 3.5f, 12100f, 10100f, 405f),
            [eStrengthClass.GL28h] = new(28f, 22.3f, 0.5f, 28.0f, 2.5f, 3.5f, 12600f, 10500f, 425f),
            [eStrengthClass.GL30h] = new(30f, 24.0f, 0.5f, 30.0f, 2.5f, 3.5f, 13600f, 11300f, 430f),
            [eStrengthClass.GL32h] = new(32f, 25.6f, 0.5f, 32.0f, 2.5f, 3.5f, 14200f, 11800f, 440f),

            [eStrengthClass.GL20c] = new(20f, 15.0f, 0.5f, 18.5f, 2.5f, 3.5f, 10400f,  8600f, 355f),
            [eStrengthClass.GL22c] = new(22f, 16.0f, 0.5f, 20.0f, 2.5f, 3.5f, 10400f,  8600f, 355f),
            [eStrengthClass.GL24c] = new(24f, 17.0f, 0.5f, 21.5f, 2.5f, 3.5f, 11000f,  9100f, 365f),
            [eStrengthClass.GL26c] = new(26f, 19.0f, 0.5f, 23.5f, 2.5f, 3.5f, 12000f, 10000f, 385f),
            [eStrengthClass.GL28c] = new(28f, 19.5f, 0.5f, 24.0f, 2.5f, 3.5f, 12500f, 10400f, 390f),
            [eStrengthClass.GL30c] = new(30f, 19.5f, 0.5f, 24.5f, 2.5f, 3.5f, 13000f, 10800f, 390f),
            [eStrengthClass.GL32c] = new(32f, 19.5f, 0.5f, 24.5f, 2.5f, 3.5f, 13700f, 11400f, 400f),
        };

        // --- NDS reference design values, declared in psi, converted to MPa on access ---
        // Representative NDS Supplement values for visually-graded dimension lumber.
        private static readonly Dictionary<eNdsSpeciesGrade, NdsReferenceValues> _nds = new()
        {
            //                                       Fb    Ft    Fv   Fc    Fc90  E         Emin
            [eNdsSpeciesGrade.DouglasFirLarch_SS]  = Nds(1500, 1000, 180, 1700, 625, 1_900_000, 690_000),
            [eNdsSpeciesGrade.DouglasFirLarch_No1] = Nds(1000,  675, 180, 1500, 625, 1_700_000, 620_000),
            [eNdsSpeciesGrade.DouglasFirLarch_No2] = Nds( 900,  575, 180, 1350, 625, 1_600_000, 580_000),
            [eNdsSpeciesGrade.HemFir_SS]           = Nds(1400,  925, 150, 1500, 405, 1_600_000, 580_000),
            [eNdsSpeciesGrade.HemFir_No1]          = Nds( 975,  625, 150, 1350, 405, 1_500_000, 550_000),
            [eNdsSpeciesGrade.HemFir_No2]          = Nds( 850,  525, 150, 1300, 405, 1_300_000, 470_000),
            [eNdsSpeciesGrade.SprucePineFir_SS]    = Nds(1250,  700, 135, 1400, 425, 1_500_000, 550_000),
            [eNdsSpeciesGrade.SprucePineFir_No1]   = Nds( 875,  450, 135, 1150, 425, 1_400_000, 510_000),
            [eNdsSpeciesGrade.SprucePineFir_No2]   = Nds( 875,  450, 135, 1150, 425, 1_400_000, 510_000),
            [eNdsSpeciesGrade.SouthernPine_SS]     = Nds(2550, 1400, 175, 2000, 565, 1_800_000, 660_000),
            [eNdsSpeciesGrade.SouthernPine_No1]    = Nds(1850, 1050, 175, 1850, 565, 1_700_000, 620_000),
            [eNdsSpeciesGrade.SouthernPine_No2]    = Nds(1500,  825, 175, 1650, 565, 1_600_000, 580_000),
        };

        private static NdsReferenceValues Nds(float fb, float ft, float fv, float fc, float fc90, float e, float emin)
            => new(fb * PsiToMpa, ft * PsiToMpa, fv * PsiToMpa, fc * PsiToMpa,
                   fc90 * PsiToMpa, e * PsiToMpa, emin * PsiToMpa);

        /// <summary>Characteristic EN 338 / EN 14080 values for a strength class (MPa).</summary>
        public static EnStrengthProperties GetEn(eStrengthClass strengthClass) => _en[strengthClass];

        /// <summary>NDS reference design values for a species + grade (MPa).</summary>
        public static NdsReferenceValues GetNds(eNdsSpeciesGrade speciesGrade) => _nds[speciesGrade];

        public static IReadOnlyCollection<eStrengthClass> EnClasses => _en.Keys;
        public static IReadOnlyCollection<eNdsSpeciesGrade> NdsGrades => _nds.Keys;
    }
}
