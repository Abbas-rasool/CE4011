namespace FrameAnalysisProgram.ANALYSIS_CORE
{
    /// <summary>
    /// Support reaction components at a restrained node, in global coordinates.
    /// A component is only meaningful (i.e. an actual reaction) when the matching
    /// DOF is restrained; the Has* flags record which ones are.
    /// </summary>
    public class NodalReaction
    {
        public int NodeId { get; }

        public double Fx { get; }
        public double Fy { get; }
        public double Mz { get; }

        public bool HasFx { get; }
        public bool HasFy { get; }
        public bool HasMz { get; }

        public NodalReaction(
            int nodeId,
            double fx, double fy, double mz,
            bool hasFx, bool hasFy, bool hasMz)
        {
            NodeId = nodeId;
            Fx = fx;
            Fy = fy;
            Mz = mz;
            HasFx = hasFx;
            HasFy = hasFy;
            HasMz = hasMz;
        }
    }
}
