using Jacobi.Vst.Plugin.Framework;
using System;

namespace TestPlugin
{
    public class PressorParams
    {
        private int _attackCounter;
        private int _releaseCounter;
        private float _sampleRate;

        private VstParameterManager _thresholdMgr;
        private VstParameterManager _ratioMgr;
        private VstParameterManager _attackMgr;
        private VstParameterManager _releaseMgr;


        public PressorParams(float sampleRate, VstParameterInfo trshInfo, VstParameterInfo ratInfo,
                            VstParameterInfo attInfo, VstParameterInfo relInfo) 
        {
            _sampleRate = sampleRate;
            _thresholdMgr = trshInfo.Normalize().ToManager();
            _ratioMgr = ratInfo.Normalize().ToManager();
            _attackMgr = attInfo.Normalize().ToManager();
            _releaseMgr = relInfo.Normalize().ToManager();
        }

        /// <summary>
        /// Lin value of db based Threshold scale
        /// </summary>
        public float Threshold { get => DBFSConvert.DbToLin(-_thresholdMgr.CurrentValue); }

        /// <summary>
        /// Dbs to Db units Ratio scale
        /// </summary>
        public float Ratio { get => _ratioMgr.CurrentValue; }

        /// <summary>
        /// State dependent ratio bringed to valid value
        /// </summary>
        public float RatioFixed => StateRatio < 1 ? 1f : StateRatio;
        /// <summary>
        /// Ratio value depending on the state
        /// </summary>
        public float StateRatio => State switch
        {
            ECompState.Bypass => Ratio,
            ECompState.Attack => Ratio * AttackCoef,
            ECompState.Release => Ratio,
            _ => 1f,
        };

        public float GainReduction { get; set; }


        /// <summary>
        /// Attack in sample units
        /// </summary>
        public float Attack => Math.Abs(_attackMgr.CurrentValue) / 1000 * _sampleRate;

        /// <summary>
        /// Release in sample units
        /// </summary>
        public float Release => Math.Abs(_releaseMgr.CurrentValue) / 1000 * _sampleRate;


        /// <summary>
        /// _attackSamplesPassed to Attack ratio
        /// </summary>
        public float AttackRatio => _attackCounter / Attack;
        
        /// <summary>
        /// _releaseSamplesHandled to Release ratio
        /// </summary>
        public float ReleaseRatio => _releaseCounter / Release;

        public float AttackCoef => (float)(1 / (Math.Exp(-4 * (AttackRatio - 1))));
        public float ReleaseCoef => (float)(Math.Exp(-4 * ReleaseRatio));



        /// <summary>
        /// Current phase of compressor
        /// </summary>
        public ECompState State { get; set; } = ECompState.Bypass;


        //public void RecountGainReduction(Sample sample)
        //{
        //    var tempGr = sample.Abs - (Threshold + ((sample.Abs - Threshold) / RatioFixed));
        //    if (tempGr > GainReduction)
        //        GainReduction = tempGr;
        //}

        public void SetSampleRate(float sR) => _sampleRate = sR;

        /// <summary>
        /// Set attack state to compressor, e.g. set attack counter to 1 or release proportion to attack, 
        /// depending on the current state, nullify release counter and set State to Attack
        /// </summary>
        internal void SetAttackState()
        {
            if (State == ECompState.Bypass)
                _attackCounter = 1;
            else if (State == ECompState.Release)
                _attackCounter = (int)(ReleaseRatio * Attack);

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
        }
    }
}
