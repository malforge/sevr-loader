using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SevrLoader
{
    /// <summary>
    ///     Resolves Space Engineers assemblies, and plugin dependencies, from folders outside the loader's own.
    /// </summary>
    /// <remarks>
    ///     Install this before any code that mentions a game type is jitted, otherwise the runtime fails to find
    ///     the assembly before the handler exists.
    /// </remarks>
    public static class GameAssemblyResolver
    {
        static readonly Dictionary<string, string> PathsByFullName = new Dictionary<string, string>(StringComparer.Ordinal);
        static readonly Dictionary<string, string> PathsBySimpleName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        static bool _installed;

        /// <summary>
        ///     Indexes every managed assembly in the given folders and starts resolving against them.
        /// </summary>
        public static void Install(params string[] folders)
        {
            foreach (var folder in folders)
            {
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                    continue;
                Index(new DirectoryInfo(folder));
            }

            if (_installed)
                return;
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
            _installed = true;
        }

        static void Index(DirectoryInfo directory)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                switch (file.Extension.ToUpperInvariant())
                {
                    case ".EXE":
                    case ".DLL":
                        break;
                    default:
                        continue;
                }

                AssemblyName name;
                try
                {
                    name = AssemblyName.GetAssemblyName(file.FullName);
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
                catch (FileLoadException)
                {
                    continue;
                }

                if (!PathsByFullName.ContainsKey(name.FullName))
                    PathsByFullName[name.FullName] = file.FullName;
                if (!PathsBySimpleName.ContainsKey(name.Name))
                    PathsBySimpleName[name.Name] = file.FullName;
            }
        }

        static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            if (PathsByFullName.TryGetValue(args.Name, out var path))
                return Assembly.LoadFrom(path);

            // The game and its plugins reference each other by versions that do not always match what is on
            // disk, so an exact-name miss falls back to the simple name.
            var simpleName = new AssemblyName(args.Name).Name;
            if (PathsBySimpleName.TryGetValue(simpleName, out path))
                return Assembly.LoadFrom(path);

            return null;
        }
    }
}
