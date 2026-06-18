namespace MemberDesigner.TimberMaterialData
{
    /// <summary>
    /// Characteristic material properties for an EN 338 / EN 14080 strength class.
    /// Strengths and moduli are in MPa (N/mm²); density is in kg/m³. These feed the EC5
    /// and TR design checks (e.g. <c>Fmk</c> → bending <c>Fm</c>, <c>Fvk</c> → <c>Fv</c>).
    /// Values follow EN 338:2016 (C/D classes) and EN 14080 (GL classes); verify against the
    /// governing edition before use in production.
    /// </summary>
    public sealed record EnStrengthProperties(
        float Fmk,    // bending strength f_m,k
        float Ft0k,   // tension parallel f_t,0,k
        float Ft90k,  // tension perpendicular f_t,90,k
        float Fc0k,   // compression parallel f_c,0,k
        float Fc90k,  // compression perpendicular f_c,90,k
        float Fvk,    // shear strength f_v,k
        float E0Mean, // mean modulus of elasticity E_0,mean
        float E005,   // 5th-percentile modulus E_0,05
        float RhoK);  // characteristic density ρ_k (kg/m³)
}
