using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework.Plugin;
using Pressor.VST;

namespace Pressor.Logic
{
    /// <summary>
    /// This class manages the plugin audio processing.
    /// </summary>
    internal sealed class AudioProcessor : VstPluginAudioProcessor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Library code")]
        private readonly Plugin _plugin;

        /// <summary>
        /// Compressor instance
        /// </summary>
        public Pressor _pressor;


        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="plugin">Must not be null.</param>
        public AudioProcessor(Plugin plugin)
            : base(2, 2, 0, noSoundInStop: true)
        {
            _plugin = plugin;
            _pressor = new Pressor(InputCount);
        }

        public PressorParameters PP => _pressor.PP;

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        /// <remarks>This property is a proxy for the <see cref="T:Pressor.PressorParameters.SampleRate"/> property.</remarks>
        public override float SampleRate
        {
            get => (float)PP.SampleRate;
            set => PP.SampleRate = value;
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
                _pressor.ProcessChannel(inChannels[i], outChannels[i], i);
        }
    }
}
