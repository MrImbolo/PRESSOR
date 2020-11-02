using Jacobi.Vst.Plugin.Framework;
using System;
using System.Drawing;

namespace TestPlugin
{
    public class PressorParams
    {
        private int _sampleCount = 0;
        private double _sampleRate;
        private double _gainReduction;
        public int SampleCount { get => _sampleCount; private set => _sampleCount = (value < int.MaxValue) ? value : 0; }

        private readonly VstParameterManager _thresholdMgr;
        private readonly VstParameterManager _ratioMgr;
        private readonly VstParameterManager _attackMgr;
        private readonly VstParameterManager _releaseMgr;
        private readonly VstParameterManager _kneeMgr;

        public PressorParams(double sampleRate, VstParameterInfo trshInfo, VstParameterInfo ratInfo,
                            VstParameterInfo attInfo, VstParameterInfo relInfo, VstParameterInfo kneeInfo) 
        {
            _sampleRate = sampleRate;
            _thresholdMgr = trshInfo.Normalize().ToManager();
            _ratioMgr = ratInfo.Normalize().ToManager();
            _attackMgr = attInfo.Normalize().ToManager();
            _releaseMgr = relInfo.Normalize().ToManager();
            _kneeMgr = kneeInfo.Normalize().ToManager();
            
        }

        /// <summary>
        /// Lin value of db based Threshold scale
        /// </summary>
        public double Threshold => -_thresholdMgr.CurrentValue;

        /// <summary>
        /// Dbs to Db units Ratio scale
        /// </summary>
        public double Ratio { get => _ratioMgr.CurrentValue; }


        /// <summary>
        /// Reduction level that must not be 0 and is a ratio between (input - threshold) / (output - threshold) difference 
        /// </summary>
        public double GainReduction {
            get => Math.Abs(_gainReduction);
            set => _gainReduction = value > Knee ? Math.Abs((value + Knee) / 2) : Math.Abs(value);
        }


        /// <summary>
        /// Attack in sample units
        /// </summary>
        public double Attack => Math.Abs(_attackMgr.CurrentValue) / 1000 * _sampleRate;

        /// <summary>
        /// Release in sample units
        /// </summary>
        public double Release => Math.Abs(_releaseMgr.CurrentValue) / 1000 * _sampleRate;


        public double Knee => _kneeMgr.CurrentValue;



        /// <summary>
        /// Current compressor curve state
        /// </summary>
        public ECurveState CurveState { get; set; } = ECurveState.None;



        /// <summary>
        /// Current phase of compressor
        /// </summary>
        public ECompState State { get; set; } = ECompState.Bypass;

        public void SetSampleRate(double sR) => _sampleRate = sR;



    }
}
