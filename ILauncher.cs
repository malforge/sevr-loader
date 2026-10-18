namespace SevrLoader
{
    /// <summary>
    ///     Starts Space Engineers inside the current process with a set of plugins registered.
    /// </summary>
    /// <remarks>
    ///     The implementation mentions game types, so it is reached through this interface and created by
    ///     reflection once <see cref="GameAssemblyResolver" /> is installed.
    /// </remarks>
    public interface ILauncher
    {
        /// <summary>
        ///     The game's Bin64 folder.
        /// </summary>
        string GameBinPath { get; set; }

        /// <summary>
        ///     Full paths of the plugin assemblies to register before the game loads.
        /// </summary>
        string[] PluginAssemblyPaths { get; set; }

        /// <summary>
        ///     Runs the game, returning when it exits.
        /// </summary>
        void Launch(string[] arguments);
    }
}
