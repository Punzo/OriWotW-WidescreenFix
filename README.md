# WidescreenFix for Ori and the Will of the Wisps

>A BepInEx 6 plugin that removes the vertical black bars and enables full 32:9 (and other super-ultrawide) support for Ori and the Will of the Wisps (Steam, 2026 build).

## Features
- Removes the pillarboxing the game applies above 21:9: the camera fills the whole screen (tested only on 32:9)
- Adds 32:9, 32:10 and 48:9 resolutions (e.g. 5120x1440) to the in-game resolution list
- Optionally switches once to your desktop resolution if the game starts fullscreen at a narrower aspect ratio
- Patches the game code at runtime: no hex editing, and it keeps working across updates as long as the method names stay the same
- Open source and easy to customize

## How it works
The game is Unity 2018.4 built with IL2CPP. Two things limit it to 21:9:
- `MaintainAspectRatio.LateUpdate` clamps the screen aspect to `[16:9, 2.389]` and shrinks `camera.rect` to that ratio, which draws the black bars. The plugin replaces it with the same logic but with no upper limit (configurable).
- `ResolutionOptions.s_allowedAspectRatios` only lists 16:9, 16:10, 5:4, 4:3 and the 21:9 variants, so 32:9 resolutions are hidden from the menu. The plugin appends the missing ratios.

## Installation
1. **Install [BepInEx 6 bleeding-edge, Unity IL2CPP x64](https://builds.bepinex.dev/projects/bepinex_be)** for the game (extract it into the folder that contains `oriwotw.exe`). BepInEx 5 does **not** work: the game is IL2CPP.
2. Launch the game once and wait: the first start takes a while as BepInEx generates its interop assemblies.
3. Build or download `WidescreenFix.dll`.
4. Place `WidescreenFix.dll` in the `BepInEx/plugins/` folder inside your game directory.
5. Launch the game and choose your 32:9 resolution in the options if it isn't selected already.

## Configuration
After the first launch, edit `BepInEx/config/com.punzo.oriwotw.widescreenfix.cfg`:

| Setting | Default | Description |
|---|---|---|
| `MaxAspectRatio` | `0` | Widest aspect ratio the camera may use. `0` = no limit. `2.389` restores the original behaviour. |
| `AddResolutions` | `true` | Add 32:9, 32:10 and 48:9 resolutions to the in-game list. |
| `UseDesktopResolution` | `true` | At startup, switch once to the desktop resolution if the game is fullscreen at a narrower aspect ratio. |

## Building from Source
You need the .NET [SDK](https://dotnet.microsoft.com/en-us/download) (6.0+) and BepInEx 6 installed in the game (step 1–2 above, so the interop assemblies exist).

1. Open a terminal in this folder.
2. Run:
	```
	dotnet build -c Release
	```
	If the game is not in the default Steam folder, add `-p:GameDir="D:\path\to\Ori and the Will of the Wisps"`.
3. The compiled DLL will be in `bin/Release/net6.0/` and is copied automatically to `BepInEx/plugins/WidescreenFix/` (disable with `-p:CopyToGame=false`).

### Project References
The `.csproj` references these DLLs from your game install:
- `BepInEx/core/BepInEx.Core.dll`, `BepInEx.Unity.IL2CPP.dll`, `0Harmony.dll`, `Il2CppInterop.Runtime.dll`
- `BepInEx/interop/UnityEngine.CoreModule.dll`, `Il2Cppmscorlib.dll`, `__mainWisp.dll`

## License
MIT License. See [LICENSE](LICENSE) for details.

## Credits
- Created by Davide Punzo
- Powered by [BepInEx](https://github.com/BepInEx/BepInEx) and [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop)

---
Pull requests and improvements welcome!
