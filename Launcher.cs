using System.Collections.Generic;
using System.IO;
using SpaceEngineers;
using VRage.FileSystem;
using VRage.Plugins;

namespace SevrLoader
{
    /// <summary>
    ///     Starts Space Engineers inside the current process with a set of plugins registered.
    /// </summary>
    public class Launcher : ILauncher
    {
        /// <inheritdoc />
        public string GameBinPath { get; set; }

        /// <inheritdoc />
        public string[] PluginAssemblyPaths { get; set; }

        /// <inheritdoc />
        public void Launch(string[] arguments)
        {
            MyFileSystem.ExePath = GameBinPath;
            MyFileSystem.RootPath = Path.GetFullPath(Path.Combine(GameBinPath, ".."));

            // MyPlugins keeps the registered assemblies in a static list, and the game only adds to it from
            // its own dedicated-server configuration. Filling it here, before MySandboxGame runs, is what
            // gets these plugins into MyPlugins.Load on a client.
            MyPlugins.RegisterUserAssemblyFiles(new List<string>(PluginAssemblyPaths));

            MyProgram.Main(arguments);
        }
    }
}
