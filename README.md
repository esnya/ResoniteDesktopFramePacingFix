# Desktop Frame Pacing Fix for Resonite

Code identity: `DesktopFramePacingFix`

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that temporarily works around desktop frame pacing problems after launching in VR.

## Scope

- Targets [#6038](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/6038): desktop `V-Sync` does not recover after VR launch -> desktop switch.
- Targets [#6345](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/6345): background frame limiting does not recover after VR launch -> desktop switch.
- Does nothing on screen-only launches where VR view is not supported from the start.

## Installation

1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Download `DesktopFramePacingFix.dll` from [GitHub Releases](https://github.com/esnya/ResoniteDesktopFramePacingFix/releases).
3. Place `DesktopFramePacingFix.dll` into your `rml_mods` directory.
4. Launch Resonite.

## Mod Settings

The mod owns its own frame pacing settings instead of writing to Resonite's built-in desktop settings:

- `Enabled`
- `ForegroundLimitEnabled`
- `MaximumForegroundFramerate`
- `BackgroundLimitEnabled`
- `MaximumBackgroundFramerate`

When `Enabled` is `false`, or when the session is not VR-capable, the mod falls back to the original engine behavior.

## Development

### Requirements

- .NET 10 SDK
- A Resonite install, or fallback assemblies under `./Resonite`
- Optional: [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) if you want hot reload

### Build

```sh
dotnet build .\DesktopFramePacingFix.slnx -c Debug -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

### Test

```sh
dotnet test .\DesktopFramePacingFix.slnx -c Debug -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

### Copy to `rml_mods`

```sh
dotnet build .\DesktopFramePacingFix.slnx -c Debug -p:CopyToMods=true -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

### Hot Reload

Hot reload is opt-in. If `ResoniteHotReloadLib.dll` and `ResoniteHotReloadLibCore.dll` are present, enable it with:

```sh
dotnet build .\DesktopFramePacingFix.slnx -c Debug -p:EnableHotReloadLibs=true -p:CopyToMods=true -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

## Versioning And Release

- Release version is derived from Git tags through `MinVer`.
- Push a tag in the form `vX.Y.Z` to create a GitHub Release for that version automatically.
- Non-tag builds keep using CI checks, but their build version is a `MinVer`-calculated pre-release version instead of a fixed repository version.
