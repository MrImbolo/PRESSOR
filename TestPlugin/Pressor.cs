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


        private ECompPhase CurrentPhase = ECompPhase.Bypass;

        private float CountPhaseRatio()
        {
            //if (CurrentPhase == ECompPhase.Bypass || CurrentPhase == ECompPhase.Compress)
            //    return Ratio;
            //else if (CurrentPhase == ECompPhase.Attack)
            //{
            //    if (_attackSamplesPassed == 0)
            //        return 1f;
            //    else 
            //        return Ratio / Ra;
            //}
            //else if (CurrentPhase == ECompPhase.Release)
            //{
            //    if (_releaseSamplesHandled == 0)
            //        return 1f; 
            //    else
            //        return Ratio / Rr;
            //}

            throw new ArgumentOutOfRangeException($"Current ENUM ${nameof(ECompPhase)} state ({CurrentPhase}) if unreacheble");
        }

        private Sample Sample { get; set; }

        private PressorParams Params { get; set; }
        
        ///// <summary>
        ///// Threshold in -DbFS
        ///// </summary>
        //private float Threshold => DBFSConvert.LinToDb(_thresholdMgr.CurrentValue);

        //private float ThresholdLin => _thresholdMgr.CurrentValue;

        ///// <summary>
        ///// In units
        ///// </summary>
        //private float Ratio => Math.Abs((int)DBFSConvert.LinToDb(_ratioMgr.CurrentValue));

        ////private float Ra
        ////{
        ////    get
        ////    {
        ////        if (_attackSamplesPassed != Attack)
        ////        {
        ////            return 1 - _attackSamplesPassed / Attack;
        ////        }
        ////        else
        ////        {
        ////            return 1f;
        ////        }
        ////    }
        ////}

        ////private float Rr
        ////{
        ////    get
        ////    {
        ////        if (_releaseSamplesHandled != Release)
        ////        {
        ////            return 1 - _releaseSamplesHandled / Release;
        ////        }
        ////        else
        ////        {
        ////            return 1f;
        ////        }
        ////    }
        ////}

        //private float Attack => (float)Math.Round(_attackMgr.CurrentValue / 1000 * SampleRate);
        //private float Release => (float)Math.Round(_releaseMgr.CurrentValue / 1000 * SampleRate);

        //private float AttackCoef => (float)Math.Exp(-Math.Log(9.0f)/(SampleRate * _attackMgr.CurrentValue));
        //private float ReleaseCoef => (float)Math.Exp(-Math.Log(9.0f)/(SampleRate * _releaseMgr.CurrentValue));

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

            //var threshMgr = threshInfo
            //    .Normalize()
            //    .ToManager();

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

            //_ratioMgr = ratInfo
            //    .Normalize()
            //    .ToManager();

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

            //_attackMgr = paramInfo
            //    .Normalize()
            //    .ToManager();

            ParameterInfos.Add(attInfo);

            var relInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                Name = "Release",
                Label = "Release",
                ShortLabel = "ms",
                MinInteger = 1,
                MaxInteger = 1000,
                StepInteger = 1,
                LargeStepInteger = 10,
                DefaultValue = 50f,
            };

            //_releaseMgr = paramInfo
            //    .Normalize()
            //    .ToManager();

            ParameterInfos.Add(relInfo);

            Params = new PressorParams(threshInfo, ratInfo, attInfo, relInfo);

            #endregion
        }
        
        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer)
        {
            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                if (inBuffer[i] == 0)
                    continue;

                Sample = new Sample(inBuffer[i]);

                if (Sample.IsZero || !Sample.IsAbove(Params.Threshold))
                    continue;

                outBuffer[i] = Sample.Compress(Params.Threshold, Params.Ratio).Value;
            }
        }
    }
    internal struct Sample
    {
        public Sample(float sample)
        {
            Value = (sample > 1)
                ? 1
                : (sample < -1)
                    ? -1
                    : sample
                ;
            Abs = Math.Abs(Value);
            Sign = (Value < 0) ? -1 : 1;
        }


        public float Value { get; private set; }
        public float Sign { get; }
        public float Abs { get; private set; }

        public bool IsAbove(float threshold) => Abs > threshold;
        public bool IsZero => Value == 0;

        public Sample Compress(float thresh, float ratio)
        {
            Abs = thresh + (Abs - thresh) / ratio;
            Value = Abs * Sign;
            return this;
        }
    }
    internal class PressorParams
    {
        private VstParameterManager _thresholdMgr;
        private VstParameterManager _ratioMgr;
        private VstParameterManager _attackMgr;
        private VstParameterManager _releaseMgr;

        public PressorParams(VstParameterInfo trshInfo, VstParameterInfo ratInfo,
                            VstParameterInfo attInfo, VstParameterInfo relInfo) 
        {
            _thresholdMgr = trshInfo.Normalize().ToManager();
            _ratioMgr = ratInfo.Normalize().ToManager();
            _attackMgr = attInfo.Normalize().ToManager();
            _releaseMgr = relInfo.Normalize().ToManager();
        }

        public float Threshold { get => DBFSConvert.DbToLin(-_thresholdMgr.CurrentValue); }
        public float Ratio { get => _ratioMgr.CurrentValue; }
    }
}
