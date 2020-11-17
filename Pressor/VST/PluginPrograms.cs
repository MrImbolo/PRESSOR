using Jacobi.Vst.Plugin.Framework;
using Jacobi.Vst.Plugin.Framework.Plugin;

namespace Pressor.VST
{
    internal sealed class PluginPrograms : VstPluginPrograms
    {
        private readonly Plugin _plugin;

        public PluginPrograms(Plugin plugin)
        {
            _plugin = plugin;
        }


        /// <summary>
        /// Initializes the plugin program collection.
        /// </summary>
        /// <returns>A filled program collection.</returns>
        protected override VstProgramCollection CreateProgramCollection()
        {
            var programs = new VstProgramCollection();

            var prog = new VstProgram(_plugin.ParameterFactory.Categories)
            {
                Name = "Default",
                IsActive = true,
            };
            
            _plugin.ParameterFactory.CreateParameters(prog.Parameters);

            programs.Add(prog);
            
            return programs;
        }
    }
}
