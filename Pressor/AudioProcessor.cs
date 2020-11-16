using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using Jacobi.Vst.Plugin.Framework.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pressor
{
    /// <summary>
    /// This class manages the plugin audio processing.
    /// </summary>
    internal sealed class AudioProcessor : VstPluginAudioProcessor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Library code")]
        private readonly Plugin _plugin;

        public PressorParameters PP { get; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="plugin">Must not be null.</param>
        public AudioProcessor(Plugin plugin)
            : base(2, 2, 0, noSoundInStop: true)
        {
            _plugin = plugin;

            PP = new PressorParameters();

            int i = 0;
            while (i < InputCount)
            {
                ChannelPressors.Add(new Pressor(PP));
                i++;
            }
        }

        /// <summary>
        /// Gets the Pressors.
        /// </summary>
        public List<Pressor> ChannelPressors { get; } = new List<Pressor>();

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        /// <remarks>This property is a proxy for the <see cref="T:Pressor.PressorParameters.SampleRate"/> property.</remarks>
        public override float SampleRate
        {
            get { return (float)PP.SampleRate; }
            set { PP.SampleRate = value; }
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

            if (inChannels.IsEmpty())
            {
                return;
            }

            for (int i = 0; i < inChannels.Length; i++)
                ChannelPressors[i].ProcessChannel(inChannels[i], outChannels[i]);
        }
    }
    public static class VstAudioBufferExtensions
    {
        public static bool IsEmpty(this VstAudioBuffer[] channels) => channels.All(x => x[0] == 0 && x[x.SampleCount - 1] == 0);
        public static double AvgEnv(this VstAudioBuffer buffer)
        {
            double[] lvls = new double[buffer.SampleCount];
            for (int i = 0; i < buffer.SampleCount; i++)
                lvls[i] = Math.Abs(buffer[i]);

            return lvls.Average();
        }
    }
}
