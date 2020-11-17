using Jacobi.Vst.Plugin.Framework;
using Jacobi.Vst.Plugin.Framework.Plugin;

namespace Pressor.VST
{
    /// <summary>
    /// The public Plugin Command Stub implementation derived from the framework provided <see cref="StdPluginCommandStub"/>.
    /// </summary>
    public class PluginCommandStub : StdPluginCommandStub
    {
        /// <summary>
        /// Called by the framework to create the plugin root class.
        /// </summary>
        /// <returns>Never returns null.</returns>
        protected override IVstPlugin CreatePluginInstance()
        {
            return new Plugin();
        }
    }
}
