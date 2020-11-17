using Jacobi.Vst.Plugin.Framework;
using Jacobi.Vst.Plugin.Framework.Plugin;
using Jacobi.Vst.Plugin.Framework.Plugin.IO;
using System.IO;
using System.Text;

namespace Pressor.VST
{
    internal sealed class PluginPersistence : VstPluginPersistence
    {
        private readonly Plugin _plugin;

        public PluginPersistence(Plugin plugin)
        {
            _plugin = plugin;
        }

        protected override VstProgramReader CreateProgramReader(Stream input)
        {
            return new PressorProgramReader(_plugin, input, Encoding);
        }

        private sealed class PressorProgramReader : VstProgramReader
        {
            private readonly Plugin _plugin;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Library code")]
            private readonly Stream _input;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Library code")]
            private readonly Encoding _encoding;

            public PressorProgramReader(Plugin plugin, Stream input, Encoding encoding)
                : base(input, encoding)
            {
                _plugin = plugin;
                _input = input;
                _encoding = encoding;
            }

            protected override VstProgram CreateProgram()
            {
                var program = new VstProgram(_plugin.ParameterFactory.Categories);

                _plugin.ParameterFactory.CreateParameters(program.Parameters);

                return program;
            }
        }
    }
}
