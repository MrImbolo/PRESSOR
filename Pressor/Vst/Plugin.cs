﻿using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Plugin;
using Jacobi.Vst.Plugin.Framework;
using Jacobi.Vst.Plugin.Framework.Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace Pressor
{
    /// <summary>
    /// Testing plugin for fun
    /// </summary>
    internal sealed class Plugin : VstPluginWithServices
    {
        public PluginParameterFactory ParameterFactory { get; set; }

        public Plugin() 
            : base("Pressor - thesis sample compressor", new FourCharacterCode("PRSR").ToInt32(), 
                  new VstProductInfo("Sound Engineering Thesis", "@MrImbolo aka Daniel Zotov @ 2020", 1002), 
                  VstPluginCategory.Mastering)
        {
            ParameterFactory = new PluginParameterFactory();
            var audioProcessor = GetInstance<AudioProcessor>();

            //ParameterFactory.ParameterInfos.AddRange(audioProcessor.ParameterInfos);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingletonAll(new AudioProcessor(this));
            services.AddSingletonAll(new PluginPersistence(this));
            services.AddSingletonAll(new PluginPrograms(this));
        }
    }
}
