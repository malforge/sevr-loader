using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SevrLoader
{
    /// <summary>
    ///     Entry point. Hosts Space Engineers in this process so plugins can be registered before it starts.
    /// </summary>
    public static class Program
    {
        const string LauncherTypeName = "SevrLoader.Launcher";

        /// <summary>
        ///     Parses the command line, then runs the game.
        /// </summary>
        [STAThread]
        public static int Main(string[] arguments)
        {
            LoaderOptions options;
            try
            {
                options = Parse(arguments);
            }
            catch (Exception exception) when (exception is ArgumentException || exception is NotSupportedException || exception is IOException)
            {
                Console.Error.WriteLine(exception.Message);
                WriteUsage();
                return 2;
            }

            // Steam identifies the app from steam_appid.txt in the working directory, which becomes the game's
            // Bin64 below, or from this variable. 244850 is Space Engineers.
            Environment.SetEnvironmentVariable("SteamAppId", "244850");

            var previousDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = options.GameBinPath;
            try
            {
                Run(options);
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
            }
            return 0;
        }

        static void Run(LoaderOptions options)
        {
            var folders = new string[options.PluginAssemblyPaths.Count + 1];
            folders[0] = options.GameBinPath;
            for (var i = 0; i < options.PluginAssemblyPaths.Count; i++)
                folders[i + 1] = Path.GetDirectoryName(options.PluginAssemblyPaths[i]);
            GameAssemblyResolver.Install(folders);

            var launcher = (ILauncher)Activator.CreateInstance(typeof(Program).Assembly.GetType(LauncherTypeName, true));
            launcher.GameBinPath = options.GameBinPath;
            launcher.PluginAssemblyPaths = options.PluginAssemblyPaths.ToArray();
            launcher.Launch(options.GameArguments.ToArray());
        }

        static LoaderOptions Parse(string[] arguments)
        {
            var options = new LoaderOptions();
            for (var i = 0; i < arguments.Length; i++)
            {
                switch (arguments[i])
                {
                    case "--game":
                        options.GameBinPath = ValueOf(arguments, ref i);
                        break;
                    case "--plugin":
                        options.PluginAssemblyPaths.Add(Path.GetFullPath(ValueOf(arguments, ref i)));
                        break;
                    default:
                        options.GameArguments.Add(arguments[i]);
                        break;
                }
            }

            options.GameBinPath = options.GameBinPath ?? Environment.GetEnvironmentVariable("SEVR_GAME_BIN") ?? FindGameBinPath();
            if (string.IsNullOrEmpty(options.GameBinPath))
                throw new ArgumentException("Space Engineers was not found. Pass --game with the path to its Bin64 folder.");
            options.GameBinPath = Path.GetFullPath(options.GameBinPath);
            if (!File.Exists(Path.Combine(options.GameBinPath, "SpaceEngineers.exe")))
                throw new ArgumentException($"\"{options.GameBinPath}\" does not contain SpaceEngineers.exe.");

            if (options.PluginAssemblyPaths.Count == 0)
                options.PluginAssemblyPaths.AddRange(FindBundledPlugins());
            if (options.PluginAssemblyPaths.Count == 0)
                throw new ArgumentException($"No plugins found in \"{PluginsFolder()}\". Pass --plugin with the path to a plugin assembly.");
            foreach (var path in options.PluginAssemblyPaths)
            {
                if (!File.Exists(path))
                    throw new ArgumentException($"Plugin assembly \"{path}\" does not exist.");
            }

            return options;
        }

        static string FindGameBinPath()
        {
            var installation = SteamLibrary.FindGameFolder("SpaceEngineers", Path.Combine("Bin64", "SpaceEngineers.exe"));
            return installation == null ? null : Path.Combine(installation, "Bin64");
        }

        static string PluginsFolder() =>
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "Plugins");

        /// <remarks>
        ///     A plugin is a folder under Plugins holding a DLL of the same name, which keeps its dependencies out
        ///     of the list, or a DLL sitting directly in Plugins.
        /// </remarks>
        static IEnumerable<string> FindBundledPlugins()
        {
            var folder = PluginsFolder();
            if (!Directory.Exists(folder))
                yield break;

            foreach (var path in Directory.GetFiles(folder, "*.dll"))
                yield return path;

            foreach (var subfolder in Directory.GetDirectories(folder))
            {
                var path = Path.Combine(subfolder, Path.GetFileName(subfolder) + ".dll");
                if (File.Exists(path))
                    yield return path;
            }
        }

        static string ValueOf(string[] arguments, ref int index)
        {
            if (index + 1 >= arguments.Length)
                throw new ArgumentException($"{arguments[index]} needs a value.");
            index++;
            return arguments[index];
        }

        static void WriteUsage()
        {
            var name = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            Console.Error.WriteLine();
            Console.Error.WriteLine($"Usage: {name} --game <Bin64> --plugin <assembly> [--plugin <assembly>] [game arguments]");
        }
    }
}
