namespace FrameAnalysis.UI.Core.Documents;

/// <summary>
/// Single change surface for the whole document. The scene/renderer pipeline and any
/// "model is dirty" tracking subscribe here instead of to individual collections or rows.
/// </summary>
public interface IDocumentChangeNotifier
{
    /// <summary>Raised when anything in the document changes — a collection edit
    /// (add/remove/move/clear) or an individual row's property (a cell edit).</summary>
    event EventHandler? Changed;
}
