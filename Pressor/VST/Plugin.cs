using Jacobi.Vst.Core;
using Jacobi.Vst.Plugin.Framework;
using Jacobi.Vst.Plugin.Framework.Plugin;
using Microsoft.Extensions.DependencyInjection;
using Pressor.Logic;

namespace Pressor.VST
{
    /// <summary>
    /// Pressor plugin derived class
    /// </summary>
    internal sealed class Plugin : VstPluginWithServices
    {
        public PluginParameterFactory ParameterFactory { get; set; }

        public Plugin() 
            : base("Pressor - thesis sample comp", new FourCharacterCode("PRSR").ToInt32(), 
                  new VstProductInfo("Sound Engineering Thesis", "@MrImbolo aka Daniel Zotov @ 2020", 1001), 
                  VstPluginCategory.Mastering)
        {
            ParameterFactory = new PluginParameterFactory();
            var audioProcessor = GetInstance<AudioProcessor>();

            ParameterFactory.ParameterInfos.AddRange(audioProcessor.PP.Parameters);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingletonAll(new AudioProcessor(this));
            services.AddSingletonAll(new PluginPersistence(this));
            services.AddSingletonAll(new PluginPrograms(this));
        }
    }
}
