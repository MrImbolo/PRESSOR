using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace TestPlugin
{
    internal enum ECompPhase
    {
        Bypass,
        Attack, 
        Compress,
        Release
    }
    internal sealed class Pressor
    {
        private int _sampleCount;
        private int _attackSamplesPassed;
        private int _releaseSamplesHandled;


        private VstParameterManager _thresholdMgr;
        private VstParameterManager _ratioMgr;
        private VstParameterManager _attackMgr;
        private VstParameterManager _releaseMgr;

        private ECompPhase CurrentPhase = ECompPhase.Bypass;

        private float CountPhaseRatio()
        {
            if (CurrentPhase == ECompPhase.Bypass || CurrentPhase == ECompPhase.Compress)
                return Ratio;
            else if (CurrentPhase == ECompPhase.Attack)
            {
                if (_attackSamplesPassed == 0)
                    return 1f;
                else 
                    return Ratio / Ra;
            }
            else if (CurrentPhase == ECompPhase.Release)
            {
                if (_releaseSamplesHandled == 0)
                    return 1f; 
                else
                    return Ratio / Rr;
            }

            throw new ArgumentOutOfRangeException($"Current ENUM ${nameof(ECompPhase)} state ({CurrentPhase}) if unreacheble");
        }

        private float _sampleDb;
        private float SampleDb
        {
            get
            {
                return _sampleDb;
            }
            set
            {
                _sampleDb = DBFSConvert.LinToDb(Math.Abs(value));
            }
        }
        
        /// <summary>
        /// Threshold in -DbFS
        /// </summary>
        private float Threshold => DBFSConvert.LinToDb(_thresholdMgr.CurrentValue);

        private float ThresholdLin => _thresholdMgr.CurrentValue;

        /// <summary>
        /// In units
        /// </summary>
        private float Ratio => Math.Abs((int)DBFSConvert.LinToDb(_ratioMgr.CurrentValue));
        private float Ra
        {
            get
            {
                if (_attackSamplesPassed != Attack)
                {
                    return 1 - _attackSamplesPassed / Attack;
                }
                else
                {
                    return 1f;
                }
            }
        }

        private float Rr
        {
            get
            {
                if (_releaseSamplesHandled != Release)
                {
                    return 1 - _releaseSamplesHandled / Release;
                }
                else
                {
                    return 1f;
                }
            }
        }

        private float Attack => (float)Math.Round(_attackMgr.CurrentValue / 1000 * SampleRate);
        private float Release => (float)Math.Round(_releaseMgr.CurrentValue / 1000 * SampleRate);

        private float AttackCoef => (float)Math.Exp(-Math.Log(9.0f)/(SampleRate * _attackMgr.CurrentValue));
        private float ReleaseCoef => (float)Math.Exp(-Math.Log(9.0f)/(SampleRate * _releaseMgr.CurrentValue));

        /// <summary>
        /// Set of parameters for the plugin
        /// </summary>
        public VstParameterInfoCollection ParameterInfos { get; internal set; }

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        public float SampleRate { get; set; }

        public Pressor()
        {
            ParameterInfos = new VstParameterInfoCollection();

            #region params

            var paramInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Thrshld",
                Label = "Threshold",
                ShortLabel = "dbs",
                MinInteger = 0,
                MaxInteger = 1,
                SmallStepFloat = 0.01f,
                StepFloat = 0.05f,
                LargeStepFloat = 0.1f,
                DefaultValue = DBFSConvert.DbToLin(-9),
            };

            _thresholdMgr = paramInfo
                .Normalize()
                .ToManager();

            ParameterInfos.Add(paramInfo);
            
            paramInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Ratio",
                Label = "Ratio",
                ShortLabel = ":1",
                MinInteger = 0,
                MaxInteger = 1,
                SmallStepFloat = 0.01f,
                StepFloat = 0.05f,
                LargeStepFloat = 0.1f,
                DefaultValue = DBFSConvert.DbToLin(-4),
            };

            _ratioMgr = paramInfo
                .Normalize()
                .ToManager();

            ParameterInfos.Add(paramInfo);

            paramInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Attack",
                Label = "Attack",
                ShortLabel = "ms",
                MinInteger = 1,
                MaxInteger = 1000,
                SmallStepFloat = 1f,
                StepFloat = 20f,
                LargeStepFloat = 100f,
                DefaultValue = 50f,
            };

            _attackMgr = paramInfo
                .Normalize()
                .ToManager();

            ParameterInfos.Add(paramInfo);

            paramInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Release",
                Label = "Release",
                ShortLabel = "ms",
                MinInteger = 1,
                MaxInteger = 1000,
                SmallStepFloat = 1f,
                StepFloat = 20f,
                LargeStepFloat = 100f,
                DefaultValue = 50f,
            };

            _releaseMgr = paramInfo
                .Normalize()
                .ToManager();

            ParameterInfos.Add(paramInfo);

            #endregion
        }
        private float ProcessSample(float sampleAbs, int sampleSign) => 
            sampleSign * (((sampleAbs - Threshold) / CountPhaseRatio()) + Threshold);



        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer)
        {
            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                var sample = inBuffer[i];

                if (sample == 0)
                {
                    outBuffer[i] = 0;
                    continue;
                }

                else if (sample > 1)
                    sample = 1f;
                
                else if (sample < -1f)
                    sample = -1f;
                

                var sampleAbs = Math.Abs(sample);

                var sampleSign = sample >= 0 ? 1 : -1;

                //SampleDb = sample;

                //CurrentPhase = ECompPhase.Compress;

                //var procSampleDb = Threshold + ((SampleDb - Threshold) / Ratio);

                //var tempRes = Math.Abs(DBFSConvert.DbToLin(procSampleDb)) * sampleSign;

                if (sampleAbs <= ThresholdLin)
                {
                    outBuffer[i] = sample;
                    continue;
                }

                var procSmpl = ThresholdLin + (sampleAbs - ThresholdLin) / Ratio;

                var tempRes = procSmpl * sampleSign;

                outBuffer[i] = tempRes;

                //_sampleCount++;

                //if (CurrentPhase == ECompPhase.Bypass)
                //{
                //    if (sampleAbs > Threshold)
                //        CurrentPhase = ECompPhase.Attack;
                //    else
                //    {
                //        outBuffer[i] = sample;
                //        continue;
                //    }
                //}
                //else if (CurrentPhase == ECompPhase.Attack)
                //{
                //    if (_attackSamplesPassed == Attack)
                //    {
                //        CurrentPhase = ECompPhase.Compress;
                //        _attackSamplesPassed = 0;
                //    }
                //    else
                //        _attackSamplesPassed++;
                //}
                //else if (CurrentPhase == ECompPhase.Compress)
                //{
                //    if (sampleAbs < Threshold)
                //    {
                //        CurrentPhase = ECompPhase.Release;
                //        _releaseSamplesHandled = 0;
                //    }
                //}
                //else if (CurrentPhase == ECompPhase.Release)
                //{
                //    if (sampleAbs > Threshold)
                //    {
                //        CurrentPhase = ECompPhase.Compress;
                //        _releaseSamplesHandled = 0;
                //    }
                //    else
                //    {
                //        _releaseSamplesHandled++;
                //        if (_releaseSamplesHandled == Release)
                //        {
                //            CurrentPhase = ECompPhase.Bypass;
                //            _releaseSamplesHandled = 0;
                //        }
                //    }
                //}

                //var processedSample = ProcessSample(sampleAbs, sampleSign);

                //var coef = Threshold + (sampleAbs - Threshold) * (1 / DBFSConvert.DbToLin(-Ratio));

                //outBuffer[i] = (float)(sample * coef);
            }
        }
    }
}
