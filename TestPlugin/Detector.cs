using System;

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
        void Detect(Sample sample);
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
        public void Detect(Sample sample)
        {
            Compute(sample);

            if (PressorParams.State == ECompState.Bypass)
            {
                if (PressorParams.GainReduction != 0)
                    PressorParams.SetAttackState();
            }
            else if (PressorParams.State == ECompState.Attack)
            {
                if (PressorParams.AttackRatio >= 1)
                    PressorParams.SetReleaseState();
            }
            else if (PressorParams.State == ECompState.Release)
            {
                if (PressorParams.GainReduction != 0)
                    PressorParams.SetAttackState();
                else if (PressorParams.ReleaseRatio >= 1)
                    PressorParams.SetBypassState();
            }
            else if (PressorParams.GainReduction != 0)
            {
                PressorParams.SetReleaseState();
            }
        }
        public void Compute(Sample sample)
        {
            // TODO: Get to know: should it update GR every time when threshold exceeded???
            if (2 * (sample.Abs - PressorParams.Threshold) < -PressorParams.Knee)
            {
                PressorParams.GainReduction = 0;
            }
            else if (2 * Math.Abs(sample.Value - PressorParams.Threshold) <= PressorParams.Knee)
            {
                PressorParams.GainReduction = (1 / PressorParams.Ratio - 1) * Math.Pow((sample.Abs - PressorParams.Threshold + PressorParams.Knee / 2), 2) / (2 * PressorParams.Knee);
            }
            else
            {
                PressorParams.GainReduction = sample.Abs - (PressorParams.Threshold + (sample.Abs - PressorParams.Threshold) / PressorParams.Ratio);
            }
        }
    }
}
