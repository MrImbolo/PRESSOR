using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace TestPlugin
{
    public enum ECompState
    {
        Bypass,
        Attack, 
        Compress,
        Release
    }
    internal sealed class Pressor
    {
        private ICompDetector _detector;
        private IGainProcessor _gainProcessor;
        private Sample _sample;
        private float _sampleRate;

        private PressorParams Params { get; set; }
        

        /// <summary>
        /// Set of parameters for the plugin
        /// </summary>
        public VstParameterInfoCollection ParameterInfos { get; internal set; }

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        public float SampleRate {
            get => _sampleRate;
            set {
                _sampleRate = value;
                Params.SetSampleRate(value);
            } 
        }

        public Pressor()
        {
            ParameterInfos = new VstParameterInfoCollection();

            #region params

            var threshInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Thrshld",
                Label = "Threshold",
                ShortLabel = "lin2dbs",
                MinInteger = 0,
                MaxInteger = 60,
                SmallStepFloat = 0.1f,
                StepFloat = 1f,
                LargeStepFloat = 3f,
                DefaultValue = DBFSConvert.DbToLin(-9),
            };

            ParameterInfos.Add(threshInfo);
            
            var ratInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Ratio",
                Label = "Ratio",
                ShortLabel = ":1",
                MinInteger = 1,
                MaxInteger = 60,
                StepInteger = 1,
                LargeStepInteger = 3,
                DefaultValue = 4f,
            };

            ParameterInfos.Add(ratInfo);

            var attInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Attack",
                Label = "Attack",
                ShortLabel = "ms",
                MinInteger = 1,
                MaxInteger = 1000,
                StepInteger = 1,
                LargeStepInteger = 10,
                DefaultValue = 50f,
            };

            ParameterInfos.Add(attInfo);

            var relInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                CanRamp = true,
                Name = "Release",
                Label = "Release",
                ShortLabel = "ms",
                MinInteger = 1,
                MaxInteger = 1000,
                StepInteger = 1,
                LargeStepInteger = 10,
                DefaultValue = 50f,
            };

            ParameterInfos.Add(relInfo);

            #endregion

            Params = new PressorParams(SampleRate, threshInfo, ratInfo, attInfo, relInfo);

            _detector = new Detector(Params);
            _gainProcessor = new GainProcessor(Params);
        }
        
        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer)
        {
            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                if (inBuffer[i] == 0)
                    continue;

                _sample = new Sample(inBuffer[i]);

                _detector.UpdateState(_sample);

                if (Params.State == ECompState.Bypass)
                    continue;

                outBuffer[i] = _gainProcessor.Process(_sample).Value;
            }
        }
    }

    public interface IGainProcessor
    {
        Sample Process(Sample sample);
    }

    internal class GainProcessor : IGainProcessor
    {
        private readonly PressorParams _params;

        public GainProcessor(PressorParams @params)
        {
            _params = @params;
        }

        public Sample Process(Sample sample)
        {
            var fixedRatio = (_params.State == ECompState.Attack) 
                ? _params.Ratio * _params.AttackCoef
                : (_params.State == ECompState.Release)
                    ? _params.Ratio * _params.ReleaseCoef
                    : _params.Ratio;

            if (fixedRatio < 1)
                fixedRatio = 1;

            // TODO: Fix this formula - this does not work well with wide range samples 
            // from release and attack state - change sample sign and so on
            // Need to divide parameters and create some range of valid values
            sample.Abs = sample.Abs >= _params.Threshold
                ? _params.Threshold + (sample.Abs - _params.Threshold) / fixedRatio
                : sample.Abs - (_params.Threshold - sample.Abs) / fixedRatio;
            
            _params.SampleHandled();

            return sample;
        }
    }

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
        /// Nullify both attack and release counters and set Compress state
        /// </summary>
        internal void SetCompressState()
        {
            _attackCounter = 0;
            _releaseCounter = 0;
            State = ECompState.Compress;
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
