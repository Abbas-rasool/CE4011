namespace FrameAnalysis.UI.Core.Documents;

/// <summary>
/// A document row that carries a 1-based identity. The id is maintained by
/// <see cref="ProjectDocument"/> to match grid order — rows never set it themselves.
/// </summary>
public interface IIdentifiedRow
{
    int Id { get; set; }
}
