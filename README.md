# Desktop Frame Pacing Fix for Resonite

Code identity: `DesktopFramePacingFix`

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that temporarily works around desktop frame pacing problems after launching in VR.

## Scope

- Targets [#6038](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/6038): desktop `V-Sync` does not recover after VR launch -> desktop switch.
- Targets [#6345](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/6345): background frame limiting does not recover after VR launch -> desktop switch.
- Does nothing on screen-only launches where VR view is not supported from the start.

## Installation

1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place `DesktopFramePacingFix.dll` into your `rml_mods` directory.
3. Launch Resonite.

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

```powershell
dotnet build .\DesktopFramePacingFix.slnx -c Debug -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

### Test

```powershell
dotnet test .\DesktopFramePacingFix.slnx -c Debug -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

### Copy to `rml_mods`

```powershell
dotnet build .\DesktopFramePacingFix.slnx -c Debug -p:CopyToMods=true -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```

### Hot Reload

Hot reload is opt-in. If `ResoniteHotReloadLib.dll` and `ResoniteHotReloadLibCore.dll` are present, enable it with:

```powershell
dotnet build .\DesktopFramePacingFix.slnx -c Debug -p:EnableHotReloadLibs=true -p:CopyToMods=true -p:ResonitePath="C:\Program Files (x86)\Steam\steamapps\common\Resonite"
```
