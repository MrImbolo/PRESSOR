﻿using Jacobi.Vst.Plugin.Framework;

namespace TestPlugin
{
    internal sealed class PluginParameterFactory
    {
        /// <summary>
        /// A collection of parameter categories.
        /// </summary>
        public VstParameterCategoryCollection Categories = new VstParameterCategoryCollection();
        /// <summary>
        /// A collection of parameter definitions.
        /// </summary>
        public VstParameterInfoCollection ParameterInfos = new VstParameterInfoCollection();

        /// <summary>
        /// Initializes the new instance.
        /// </summary>
        public PluginParameterFactory()
        {
            VstParameterCategory paramCat = new VstParameterCategory
            {
                Name = "Compressor"
            };

            Categories.Add(paramCat);
        }

        /// <summary>
        /// Fills the <paramref name="parameters"/> collection with all parameters.
        /// </summary>
        /// <param name="parameters">Must not be null.</param>
        /// <remarks>A <see cref="VstParameter"/> instance is created and linked up for each
        /// <see cref="VstParameterInfo"/> instance found in the <see cref="ParameterInfos"/> collection.</remarks>
        public void CreateParameters(VstParameterCollection parameters)
        {
            foreach (VstParameterInfo paramInfo in ParameterInfos)
            {
                if (Categories.Count > 0 && paramInfo.Category == null)
                {
                    paramInfo.Category = Categories[0];
                }

                var param = new VstParameter(paramInfo);
                parameters.Add(param);
            }
        }
    }
}
