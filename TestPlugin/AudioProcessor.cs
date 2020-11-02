using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework.Plugin;
using System;
using System.Linq;

namespace TestPlugin
{
    /// <summary>
    /// This class manages the plugin audio processing.
    /// </summary>
    internal sealed class AudioProcessor : VstPluginAudioProcessor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Library code")]
        private readonly Plugin _plugin;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="plugin">Must not be null.</param>
        public AudioProcessor(Plugin plugin)
            : base(2, 2, 0, noSoundInStop: true)
        {
            _plugin = plugin;
            Pressor = new Pressor();
        }

        /// <summary>
        /// Gets the Pressor.
        /// </summary>
        public Pressor Pressor { get; }

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        /// <remarks>This property is a proxy for the <see cref="T:Jacobi.Vst.Samples.Delay.Delay.SampleRate"/> property.</remarks>
        public override float SampleRate
        {
            get { return Pressor.SampleRate; }
            set { Pressor.SampleRate = value; }
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
                Pressor.ProcessChannel(inChannels[i], outChannels[i]);
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
    }
}
