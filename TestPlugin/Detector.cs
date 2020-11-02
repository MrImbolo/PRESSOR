using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace TestPlugin
{
    public interface ICompDetector
    {
        /// <summary>
        /// Update compressor state, depending on the current sample's value
        /// </summary>
        /// <param name="sample">Current audio sample</param>
        void Detect(Sample sample);
        /// <summary>
        /// Is detector in bypass or release state and waiting for level excess  detection
        /// </summary>
        /// <returns>Whether is in waiting mode or not</returns>
        bool IsWaitingForDetection();
        /// <summary>
        /// Did detector find out that current sample should be processed by GainProcessor
        /// </summary>
        /// <returns></returns>
        bool IsCurrentSampleForProcessing();
    }

    /// <summary>
    /// Incapsulated detector logic
    /// </summary>
    internal class Detector : ICompDetector
    {
        /// <summary>
        /// Current compressor parameters
        /// </summary>
        public PressorParams _pressorParams;
        private readonly IStateHandler _stateHandler;

        public Detector(PressorParams @params, IStateHandler stateHandler)
        {
            _pressorParams = @params;
            _stateHandler = stateHandler;
        }

        /// <summary>
        /// Update compressor state, depending on the current sample's value. 
        /// Works only if current state is bypass or release
        /// </summary>
        /// <param name="sample">Current audio sample</param>
        public void Detect(Sample sample)
        {
            CheckExceesAndCountGR(sample);

            // In the end _pressorParams.ECompState must be changed and 
            // GainReduction value too

            if (IsDetected())
            {
                _stateHandler.SetAttackState();
            }
        }


        public bool IsCurrentSampleForProcessing() => _stateHandler.State != ECompState.Bypass;

        public bool IsWaitingForDetection() => _stateHandler.State == ECompState.Bypass;

        private bool IsDetected() => _pressorParams.CurveState != ECurveState.None;

        private void CheckExceesAndCountGR(Sample sample)
        {
            if (2 * (sample.Abs - _pressorParams.Threshold) < -_pressorParams.Knee)
            {
                _pressorParams.GainReduction = 0;
                _pressorParams.CurveState = ECurveState.None;
            }
            else if (2 * Math.Abs(sample.Abs - _pressorParams.Threshold) <= _pressorParams.Knee)
            {
                var tempGr = (1 / _pressorParams.Ratio - 1) * Math.Pow(sample.Abs - _pressorParams.Threshold + _pressorParams.Knee / 2, 2) / (2 * _pressorParams.Knee);
                _pressorParams.GainReduction = tempGr;
                _pressorParams.CurveState = ECurveState.Knee;
            }
            else
            {
                var tempGr = sample.Abs - (_pressorParams.Threshold + (sample.Abs - _pressorParams.Threshold) / _pressorParams.Ratio);
                _pressorParams.GainReduction = tempGr;
                _pressorParams.CurveState = ECurveState.Linear;
            }
        }
    }
}
