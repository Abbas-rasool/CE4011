namespace FrameAnalysis.UI.Core.Rendering;

/// <summary>
/// Draws a <see cref="Scene"/>. This is the only contract between the structural model and
/// the drawing technology: implementations (a WPF 2D canvas now, a 3D library later) live
/// in the UI layer and are the sole place that references a drawing framework. Swapping 2D
/// for 3D means a new implementation here — the document, mapper, and scene are untouched.
/// </summary>
public interface IStructuralRenderer
{
    /// <summary>Render the given scene, replacing whatever was drawn before.</summary>
    void Render(Scene scene);

    /// <summary>
    /// Hit-test a screen-space point against the rendered members; returns the element id
    /// under the point, or null. This is the canvas-click half of the hybrid selection model
    /// (it just sets the selected element id, same as picking from the sidebar list).
    /// </summary>
    int? HitTestElement(double screenX, double screenY);
}
