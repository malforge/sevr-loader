using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SevrLoader
{
    /// <summary>
    ///     Finds games installed through Steam, across every library folder on the machine.
    /// </summary>
    public static class SteamLibrary
    {
        // libraryfolders.vdf lists each library as a block carrying a "path" entry; the key is quoted and so is
        // its value. Only the paths are needed here, so the file is read for those rather than parsed in full.
        static readonly Regex PathEntry = new Regex("\"path\"\\s+\"([^\"]*)\"", RegexOptions.Compiled);

        /// <summary>
        ///     Finds an installed game's folder, or null when it is not installed or Steam cannot be located.
        /// </summary>
        /// <param name="folderName">The game's folder name under steamapps\common.</param>
        /// <param name="verificationPath">A path within the game folder whose presence confirms the match.</param>
        public static string FindGameFolder(string folderName, string verificationPath)
        {
            foreach (var library in Libraries())
            {
                var candidate = Path.Combine(library, "steamapps", "common", folderName);
                if (File.Exists(Path.Combine(candidate, verificationPath)))
                    return candidate;
            }
            return null;
        }

        static IEnumerable<string> Libraries()
        {
            var steamPath = SteamPath();
            if (string.IsNullOrEmpty(steamPath))
                yield break;

            yield return steamPath;

            var configuration = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(configuration))
                yield break;

            string content;
            try
            {
                content = File.ReadAllText(configuration);
            }
            catch (IOException)
            {
                yield break;
            }

            foreach (Match match in PathEntry.Matches(content))
            {
                // Paths in the file are escaped for the vdf format.
                var path = match.Groups[1].Value.Replace("\\\\", "\\");
                if (!string.IsNullOrEmpty(path))
                    yield return path;
            }
        }

        static string SteamPath()
        {
            try
            {
                return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
