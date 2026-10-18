# SevrLoader

Runs [Space Engineers VR](https://github.com/ColbyJack1134/SpaceEngineersVR) without a plugin
loader, by hosting the game inside its own process.

Space Engineers only loads plugins from the command line on a dedicated server. On the client it
ignores them unless the first `-plugin` value matches a value the game checks for, so a plugin that
works perfectly well has no supported way in. This loader sidesteps that: it starts the game's own
entry point in-process and registers the plugin through `MyPlugins.RegisterUserAssemblyFiles`
before the game gets to its own registration.

That approach is the one used by the [MDK²](https://github.com/malforge/mdk2) extractor.

## Why

Because the code you reviewed should be the code you run. A loader that fetches and updates plugins
on its own schedule means the version you vetted is not necessarily the version that starts
tomorrow. This runs a build you made, from a commit you chose, and nothing else.

See **[is-the-space-engineers-vr-plugin-safe.markdown](is-the-space-engineers-vr-plugin-safe.markdown)**
for a security review of the plugin at commit `5f039a9`.

## Use

```
SevrLoader.exe [--game <Bin64>] [--plugin <assembly>] [game arguments]
```

With no arguments it finds Space Engineers through Steam and loads every plugin under `Plugins`.
Lay it out like this:

```
SevrLoader.exe
steam_appid.txt
Plugins/
  SpaceEngineersVR/
    SpaceEngineersVR.dll
    openvr_api.dll
    0Harmony.dll
    ...
    SEVRAssets/
```

A plugin is a folder under `Plugins` holding a DLL of the same name, or a DLL directly in
`Plugins`. Dependencies sit beside the plugin and are resolved from there.

`--plugin` may be repeated. Anything the loader does not recognise is passed to the game, so
`-nosplash -skipintro` and friends work as usual.

## Build

```
dotnet build SevrLoader.csproj -c Release -p:GameBinPath="...\SpaceEngineers\Bin64"
```

The game's `SpaceEngineers.exe` and `VRage.Library.dll` are referenced from that folder and not
redistributed.

### Building the plugin

Build Space Engineers VR with its diagnostics harness left out. Its project file switches that on
unconditionally, which ships a test harness capable of loading arbitrary assemblies into your game:

```
dotnet build SpaceEngineersVR\SpaceEngineersVR.csproj -c Release ^
  -p:GameBinPath="...\SpaceEngineers\Bin64" -p:DefineConstants=""
```

Copy the output into `Plugins\SpaceEngineersVR`.

## Notes

- Requires Steam to be running, and SteamVR for the VR plugin to do anything.
- The game runs *inside* `SevrLoader.exe`; there is no separate `SpaceEngineers` process.
- Nothing is written into the game install.

## Licence

MIT. Space Engineers is a trademark of Keen Software House; this project is not affiliated with
Keen, nor with the author of Space Engineers VR.
