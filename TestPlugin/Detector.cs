namespace TestPlugin
{
    public interface ICompDetector
    {
        /// <summary>
        /// Current compressor parameters
        /// </summary>
        PressorParams PressorParams { get; }

        /// <summary>
        /// Update compressor state, depending on the current sample's value
        /// </summary>
        /// <param name="sample">Current audio sample</param>
        void UpdateState(Sample sample);
    }

    /// <summary>
    /// Incapsulated detector logic
    /// </summary>
    internal class Detector : ICompDetector
    {
        public Detector(PressorParams @params)
        {
            PressorParams = @params;
        }

        /// <summary>
        /// Current compressor parameters
        /// </summary>
        public PressorParams PressorParams { get; }

        /// <summary>
        /// Update compressor state, depending on the current sample's value
        /// </summary>
        /// <param name="sample">Current audio sample</param>
        public void UpdateState(Sample sample)
        {
            if (PressorParams.State == ECompState.Bypass)
            {
                if (sample.IsAbove(PressorParams.Threshold))
                    PressorParams.SetAttackState();
            }
            else if (PressorParams.State == ECompState.Attack)
            {
                if (PressorParams.AttackRatio >= 1)
                    PressorParams.SetReleaseState();
            }
            else if (PressorParams.State == ECompState.Release)
            {
                if (sample.IsAbove(PressorParams.Threshold))
                    PressorParams.SetAttackState();
                else if (PressorParams.ReleaseRatio >= 1)
                    PressorParams.SetBypassState();
            }
        }
    }
}
