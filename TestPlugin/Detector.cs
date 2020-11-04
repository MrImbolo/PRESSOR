using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace TestPlugin
{
    public interface ICompDetector
    {
        ///// <summary>
        ///// Update compressor state, depending on the current sample's value
        ///// </summary>
        ///// <param name="sample">Current audio sample</param>
        //void Detect(Sample sample);
        bool DetectByEnv();
    }

    /// <summary>
    /// Incapsulated detector logic
    /// </summary>
    internal class Detector
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

        ///// <summary>
        ///// Update compressor state, depending on the current sample's value. 
        ///// Works only if current state is bypass or release
        ///// </summary>
        ///// <param name="sample">Current audio sample</param>
        //public void Detect(Sample sample)
        //{
        //    CheckExceesAndCountGR(sample.Abs);

        //    // In the end _pressorParams.ECompState must be changed and 
        //    // GainReduction value too

        //    if (IsDetected())
        //    {
        //        _stateHandler.SetAttackState();
        //    }
        //}

        private void CheckExceesAndCountGR(double sample)
        {
            if (2 * (sample - _pressorParams.T) < -_pressorParams.W)
            {
                _pressorParams.GainReduction = 0;
            }
            else if (2 * Math.Abs(sample - _pressorParams.T) <= _pressorParams.W)
            {
                var tempGr = (1 / _pressorParams.R - 1) * Math.Pow(sample - _pressorParams.T + _pressorParams.W / 2, 2) / (2 * _pressorParams.W);
                _pressorParams.GainReduction = tempGr;
            }
            else
            {
                var tempGr = sample - (_pressorParams.T + (sample - _pressorParams.T) / _pressorParams.R);
                _pressorParams.GainReduction = tempGr;
            }
        }

        //public bool DetectByEnv()
        //{
        //    if (2 * (_pressorParams.Env.Dbs - _pressorParams.T) < -_pressorParams.W)
        //    {
        //        _pressorParams.GainReduction = 0;
        //        return false;
        //    }
        //    else if (2 * Math.Abs(_pressorParams.Env.Dbs - _pressorParams.T) <= _pressorParams.W)
        //    {
        //        _pressorParams.GainReduction = CountGRByKnee();
        //    }
        //    else
        //    {
        //        _pressorParams.GainReduction = CountGR();
                    
        //    }
        //    return true;
        //}
        //private double CountGRByKnee() => 
        //    (1 / _pressorParams.R - 1) * Math.Pow(_pressorParams.Env.Dbs - _pressorParams.T + _pressorParams.W / 2, 2) / (2 * _pressorParams.W);
        //private double CountGR() => 
        //    _pressorParams.Env.Dbs - (_pressorParams.T + (_pressorParams.Env.Dbs - _pressorParams.T) / _pressorParams.R);

    }
}
