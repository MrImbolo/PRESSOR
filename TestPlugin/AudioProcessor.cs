using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using Jacobi.Vst.Plugin.Framework.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace TestPlugin
{
    /// <summary>
    /// This class manages the plugin audio processing.
    /// </summary>
    internal sealed class AudioProcessor : VstPluginAudioProcessor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Library code")]
        private readonly Plugin _plugin;

        public VstParameterInfoCollection ParameterInfos { get; }

        public PressorParams PP { get; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="plugin">Must not be null.</param>
        public AudioProcessor(Plugin plugin)
            : base(2, 2, 0, noSoundInStop: true)
        {
            _plugin = plugin;

            ParameterInfos = new VstParameterInfoCollection();
            InitializeParameterInfos();

            PP = new PressorParams(ParameterInfos);

            int i = 0;
            while(i < InputCount)
            {
                PressorChannels.Add(new Pressor(PP));
                i++;
            }
        }

        /// <summary>
        /// Gets the Pressor.
        /// </summary>
        public List<Pressor> PressorChannels { get; } = new List<Pressor>();

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        /// <remarks>This property is a proxy for the <see cref="T:Jacobi.Vst.Samples.Delay.Delay.SampleRate"/> property.</remarks>
        public override float SampleRate
        {
            get { return (float)PP.SampleRate; }
            set { PP.SampleRate = value; }
        }


        private void InitializeParameterInfos()
        {
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
                DefaultValue = (float)DBFSConvert.DbToLin(-9),
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


            var kneeInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                CanRamp = true,
                Name = "Knee",
                Label = "Knee",
                ShortLabel = "db to db",
                MinInteger = 0,
                MaxInteger = 10,
                StepInteger = 1,
                LargeStepInteger = 1,
                DefaultValue = 1,
            };
            ParameterInfos.Add(kneeInfo);

            var mGainInfo = new VstParameterInfo
            {
                CanBeAutomated = true,
                CanRamp = true,
                Name = "MkGain",
                Label = "MakeUpGain",
                ShortLabel = "dbs",
                MinInteger = 0,
                MaxInteger = 60,
                StepInteger = 1,
                LargeStepInteger = 1,
                DefaultValue = 0,
            };
            ParameterInfos.Add(mGainInfo);

            #endregion
        }


        /// <summary>
        /// Perform audio processing on the specified <paramref name="inChannels"/> 
        /// and produce an effect on the <paramref name="outChannels"/>.
        /// </summary>
        /// <param name="inChannels">The audio input buffers.</param>
        /// <param name="outChannels">The audio output buffers.</param>
        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            base.Process(inChannels, outChannels);

            if (inChannels.All(x => x[0] == 0))
                return;

            for (int i = 0; i < inChannels.Length; i++)
                PressorChannels[i].ProcessChannel(inChannels[i], outChannels[i]);
        }
    }
    public static class VstAudioBufferExtensions
    {
        public static bool IsEmpty(this VstAudioBuffer buffer)
        {
            for (int i = 0; i < buffer.SampleCount; i++)
                if (buffer[i] != 0) return false;

            return true;
        }
        public static double AvgEnv(this VstAudioBuffer buffer)
        {
            double[] lvls = new double[buffer.SampleCount];
            for (int i = 0; i < buffer.SampleCount; i++)
                lvls[i] = Math.Abs(buffer[i]);

            return lvls.Average();
        }
    }
}
