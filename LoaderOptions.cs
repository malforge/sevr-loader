using System.Collections.Generic;

namespace SevrLoader
{
    /// <summary>
    ///     What the loader was asked to run.
    /// </summary>
    public class LoaderOptions
    {
        /// <summary>
        ///     The game's Bin64 folder.
        /// </summary>
        public string GameBinPath { get; set; }

        /// <summary>
        ///     Full paths of the plugin assemblies to register.
        /// </summary>
        public List<string> PluginAssemblyPaths { get; } = new List<string>();

        /// <summary>
        ///     Arguments passed through to the game untouched.
        /// </summary>
        public List<string> GameArguments { get; } = new List<string>();
    }
}
