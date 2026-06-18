namespace MemberDesigner.TimberMaterialData
{
    /// <summary>
    /// NDS reference design values for a visually-graded solid-sawn species + grade.
    /// Stored in MPa (N/mm²) — the US design checks work in SI internally and convert to
    /// imperial only where a code formula requires it (see <c>TimberDesCheckBendingUS</c>).
    /// Source values are the NDS Supplement reference design values (psi), converted with
    /// <see cref="TimberMaterialDatabase.PsiToMpa"/>. Representative values only — verify
    /// against the current NDS Supplement before production use.
    /// </summary>
    public sealed record NdsReferenceValues(
        float Fb,   // reference bending design value
        float Ft,   // reference tension parallel
        float Fv,   // reference shear parallel
        float Fc,   // reference compression parallel
        float Fc90, // reference compression perpendicular
        float E,    // modulus of elasticity
        float Emin);// adjusted (buckling) modulus of elasticity
}
