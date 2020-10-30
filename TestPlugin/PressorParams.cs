using Jacobi.Vst.Plugin.Framework;
using System;

namespace TestPlugin
{
    public class PressorParams
    {
        private int _sampleCount = 0;
        private int _attackCounter;
        private int _releaseCounter;
        private double _sampleRate;
        private double _gainReduction;
        public int SampleCount { get => _sampleCount; private set => _sampleCount = (value < int.MaxValue) ? value : 0; }

        private VstParameterManager _thresholdMgr;
        private VstParameterManager _ratioMgr;
        private VstParameterManager _attackMgr;
        private VstParameterManager _releaseMgr;
        private VstParameterManager _kneeMgr;

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
        public double Threshold { get => DBFSConvert.DbToLin(-_thresholdMgr.CurrentValue); }

        /// <summary>
        /// Dbs to Db units Ratio scale
        /// </summary>
        public double Ratio { get => _ratioMgr.CurrentValue; }

        /// <summary>
        /// State dependent ratio bringed to valid value
        /// </summary>
        // public double RatioFixed => StateRatio < 1 ? 1f : StateRatio;

        /// <summary>
        /// Ratio value depending on the state
        /// </summary>
        //public double StateRatio => State switch
        //{
        //    ECompState.Bypass => Ratio,
        //    ECompState.Attack => 1 + Ratio * AttackCoef > Ratio ? Ratio : 1 + Ratio * AttackCoef,
        //    ECompState.Release => Ratio,
        //    _ => 1f,
        //};


        /// <summary>
        /// Reduction level that must not be 0 and is a ratio between (input - threshold) / (output - threshold) difference 
        /// </summary>
        public double GainReduction {
            get => Math.Abs(_gainReduction);
            set => _gainReduction = value > Knee ? Math.Abs((value + Knee) / 2) : Math.Abs(value);
        }
        /// <summary>
        /// Reduction level that must not be 0 and is a ratio between (input - threshold) / (output - threshold) difference 
        /// </summary>
        public double LastGainReduction { get; set; }


        /// <summary>
        /// Attack in sample units
        /// </summary>
        public double Attack => Math.Abs(_attackMgr.CurrentValue) / 1000 * _sampleRate;

        /// <summary>
        /// Release in sample units
        /// </summary>
        public double Release => Math.Abs(_releaseMgr.CurrentValue) / 1000 * _sampleRate;


        /// <summary>
        /// _attackSamplesPassed to Attack ratio
        /// </summary>
        public double AttackRatio => _attackCounter / Attack;
        
        /// <summary>
        /// _releaseSamplesHandled to Release ratio
        /// </summary>
        public double ReleaseRatio => _releaseCounter / Release;

        //public double AttackCoef => 1 / (Math.Exp(-4 * (AttackRatio - 1)));
        //public double ReleaseCoef => Math.Exp(-4 * ReleaseRatio);
        public double Knee => DBFSConvert.DbToLin(-_kneeMgr.CurrentValue);

        public double GainReductionFixed => State switch 
        { 
            ECompState.Attack => GainReduction * AttackRatio,
            ECompState.Release => GainReduction * ReleaseRatio,
            _ => GainReduction
        };


        /// <summary>
        /// Current phase of compressor
        /// </summary>
        public ECompState State { get; set; } = ECompState.Bypass;

        public void SetSampleRate(double sR) => _sampleRate = sR;

        /// <summary>
        /// Set attack state to compressor, e.g. set attack counter to 1 or release proportion to attack, 
        /// depending on the current state, nullify release counter and set State to Attack
        /// </summary>
        internal void SetAttackState()
        {
            if (State == ECompState.Bypass)
                _attackCounter = 1;
            else if (State == ECompState.Release)
                _attackCounter = (int)Math.Round(ReleaseRatio * Attack, 0);

            _releaseCounter = 0;
            State = ECompState.Attack;
        }

        /// <summary>
        /// Nullify attack counter, set release counter to 1 and set Release state
        /// </summary>
        internal void SetReleaseState()
        {
            _attackCounter = 0;
            _releaseCounter = 1;

            State = ECompState.Release;
        }

        /// <summary>
        /// Nullify both attack and release counters and set Bypass state
        /// </summary>
        internal void SetBypassState()
        {
            GainReduction = 0;
            _releaseCounter = 0;
            _attackCounter = 0;
            State = ECompState.Bypass;
        }

        internal void SampleHandled()
        {
            if (State == ECompState.Attack)
                _attackCounter++;

            if (State == ECompState.Release)
                _releaseCounter++;

            LastGainReduction = GainReductionFixed;
            GainReduction = 0d;
        }
    }
}
