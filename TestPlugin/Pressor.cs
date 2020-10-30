using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace TestPlugin
{
    public enum ECompState
    {
        Bypass,
        Attack, 
        Release
    }
    internal sealed class Pressor
    {
        private ICompDetector _detector;
        private IGainProcessor _gainProcessor;
        private Sample _sample;
        private float _sampleRate;
        private int _sampleCount = 0;


        public int SampleCount { get => _sampleCount; private set => _sampleCount = (value < int.MaxValue) ? value : 0; }
        private PressorParams PressorParams { get; set; }
        

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
                PressorParams.SetSampleRate(value);
            } 
        }

        public Pressor()
        {
            InitializeParameterInfos();
            _detector = new Detector(PressorParams);
            _gainProcessor = new GainProcessor(PressorParams);
        }

        private void InitializeParameterInfos()
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


            var kneeInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                CanRamp = true,
                Name = "Knee",
                Label = "Knee",
                ShortLabel = "db to db",
                MinInteger = 1,
                MaxInteger = 10,
                StepInteger = 1,
                LargeStepInteger = 1,
                DefaultValue = 1,
            };
            ParameterInfos.Add(kneeInfo);

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

            PressorParams = new PressorParams(SampleRate, threshInfo, ratInfo, attInfo, relInfo, kneeInfo);
        }

        public void ProcessChannel(VstAudioBuffer inBuffer, VstAudioBuffer outBuffer)
        {
            for (var i = 0; i < inBuffer.SampleCount; i++)
            {
                if (inBuffer[i] == 0)
                {
                    PressorParams.SetBypassState();
                    continue;
                }

                _sample = new Sample(inBuffer[i]);
                _detector.Detect(_sample);

                outBuffer[i] = (float)_gainProcessor.Process(_sample).Value;
            }
        }
    }

    
}
