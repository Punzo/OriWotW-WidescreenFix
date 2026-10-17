# WidescreenFix for Ori and the Will of the Wisps

>A BepInEx 6 plugin that removes the vertical black bars and enables full 32:9 (and other super-ultrawide) support for Ori and the Will of the Wisps (Steam, 2026 build).

![Screenshot at 5120x1440](https://github.com/Punzo/OriWotW-WidescreenFix/raw/main/Screenshot.png)

## Why another fix?
The previously available ultrawide fixes no longer work with the current Steam version of the game (2026 build):
- **[WSGF hex edit by Helifax](https://www.wsgf.org/node/30070)** (`60 E5 18 40` → `00 00 10 41` in `GameAssembly.dll`): patches the camera aspect limit, but on its own it does not remove the black bars on the current build, because 32:9 resolutions are still hidden from the in-game menu.
- **[PCGamingWiki "Superwide & Multimonitor Fix"](https://community.pcgamingwiki.com/files/file/1839-ori-and-the-will-of-the-wisps-superwide-multimonitor-fix/)**: made for the older Game Pass / Microsoft Store version, not the current Steam build.

This plugin patches the game at runtime instead of hex-editing the executable, so it does not depend on byte offsets that change between game updates.

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
1. Download `OriWotW-WidescreenFix-x.y.z.zip` from the [Releases](https://github.com/Punzo/OriWotW-WidescreenFix/releases) page.
2. Extract it into the game folder, the one that contains `oriwotw.exe` (in Steam: right-click the game → *Manage* → *Browse local files*).
3. Launch the game. **The first start takes a few minutes** (black screen or no window) while BepInEx prepares itself; it needs an internet connection that one time. Later starts are normal.
4. Choose your 32:9 resolution in the options if it isn't selected already.

The zip already contains [BepInEx 6](https://github.com/BepInEx/BepInEx) (bleeding-edge build be.789, Unity IL2CPP x64), set up with its console window disabled. If you already have BepInEx 6 installed, you can instead copy only `WidescreenFix.dll` into `BepInEx/plugins/`. BepInEx 5 does **not** work: the game is IL2CPP.

To uninstall, delete `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version` and the `BepInEx` and `dotnet` folders from the game folder.

### Linux / Steam Deck (Proton) (untested)
The game is Windows-only on Steam, so on Linux and Steam Deck it runs through Proton, and the same zip is used.
1. Install as above: extract the zip into the game folder.
2. In Steam, right-click the game → *Properties* → *Launch Options*, and enter:
	```
	WINEDLLOVERRIDES="winhttp=n,b" %command%
	```
	Without this, Proton uses its own `winhttp.dll` and BepInEx (and the fix) never loads.
3. Launch the game. The first start takes a few minutes, as on Windows.

This is the standard way to run BepInEx under Proton, but it has not been tested with this game yet. Feedback is welcome.

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

To create the release zip (BepInEx + plugin, ready to extract), run:
```
powershell -ExecutionPolicy Bypass -File package.ps1
```
It builds the plugin, downloads the pinned BepInEx version into `.cache/`, and writes `release/OriWotW-WidescreenFix-<version>.zip` and `release/WidescreenFix.dll`.

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
