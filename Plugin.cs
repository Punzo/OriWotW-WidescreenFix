using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

// Ori and the Will of the Wisps (Unity 2018.4.24, IL2CPP) pillarboxes anything wider than ~21:9:
//  - MaintainAspectRatio.LateUpdate clamps Screen.width/height to [16:9, 2.389] and shrinks camera.rect to that ratio.
//  - ResolutionOptions.s_allowedAspectRatios has no 32:9 entry, so 5120x1440 never shows up in the options menu.
[BepInPlugin("com.punzo.oriwotw.widescreenfix", "Widescreen Fix 32:9", "1.0.0")]
public class WidescreenFix : BasePlugin
{
    internal static new ManualLogSource Log;
    internal static ConfigEntry<float> MaxAspectRatio;
    internal static ConfigEntry<bool> AddResolutions;
    internal static ConfigEntry<bool> UseDesktopResolution;

    public override void Load()
    {
        Log = base.Log;
        MaxAspectRatio = Config.Bind("General", "MaxAspectRatio", 0f,
            "Widest aspect ratio the camera may use. 0 = no limit (fill the whole screen). The game's own limit is 2.389 (21:9).");
        AddResolutions = Config.Bind("General", "AddResolutions", true,
            "Add 32:9, 32:10 and 48:9 resolutions to the in-game resolution list.");
        UseDesktopResolution = Config.Bind("General", "UseDesktopResolution", true,
            "At startup, if the game is fullscreen at a narrower aspect ratio than the desktop, switch once to the desktop resolution.");

        Harmony.CreateAndPatchAll(typeof(Patches));
        AddComponent<WidescreenFixBehaviour>();
        Log.LogInfo($"WidescreenFix loaded. MaxAspectRatio={MaxAspectRatio.Value}, AddResolutions={AddResolutions.Value}, UseDesktopResolution={UseDesktopResolution.Value}");
    }
}

internal static class Patches
{
    const float MinAspect = 16f / 9f;
    static bool s_loggedCamera;
    static bool s_addedRatios;

    // Same maths as the original method, only with a configurable upper bound.
    [HarmonyPatch(typeof(MaintainAspectRatio), nameof(MaintainAspectRatio.LateUpdate))]
    [HarmonyPrefix]
    static bool MaintainAspectRatioLateUpdate(MaintainAspectRatio __instance)
    {
        try
        {
            Camera cam = __instance.m_camera;
            if (cam == null || Screen.height <= 0)
                return true;

            float screenAspect = (float)Screen.width / Screen.height;
            float max = WidescreenFix.MaxAspectRatio.Value > 0f ? WidescreenFix.MaxAspectRatio.Value : float.MaxValue;
            float target = Mathf.Clamp(screenAspect, MinAspect, Mathf.Max(max, MinAspect));
            float ratio = screenAspect / target;

            Rect rect = ratio >= 1f
                ? new Rect(0.5f - 0.5f / ratio, 0f, 1f / ratio, 1f)   // wider than target: pillarbox
                : new Rect(0f, 0.5f - 0.5f * ratio, 1f, ratio);       // narrower than 16:9: letterbox
            cam.rect = rect;

            if (!s_loggedCamera)
            {
                s_loggedCamera = true;
                WidescreenFix.Log.LogInfo($"MaintainAspectRatio patched: camera '{cam.name}', screen {Screen.width}x{Screen.height} ({screenAspect:0.###}), rect {rect}");
            }
            return false;
        }
        catch (Exception ex)
        {
            WidescreenFix.Log.LogError($"MaintainAspectRatio patch failed, falling back to original: {ex}");
            return true;
        }
    }

    [HarmonyPatch(typeof(ResolutionOptions), nameof(ResolutionOptions.Awake))]
    [HarmonyPrefix]
    static void ResolutionOptionsAwake() => AddAllowedAspectRatios();

    [HarmonyPatch(typeof(ResolutionOptions), nameof(ResolutionOptions.OnEnable))]
    [HarmonyPrefix]
    static void ResolutionOptionsOnEnable() => AddAllowedAspectRatios();

    // Entries are (matchWidth, matchHeight, labelWidth, labelHeight), e.g. (43, 18, 21, 9) lists 3440x1440 as "21:9".
    static void AddAllowedAspectRatios()
    {
        if (s_addedRatios || !WidescreenFix.AddResolutions.Value)
            return;
        s_addedRatios = true;
        try
        {
            var extra = new[] { new Vector4(32, 9, 32, 9), new Vector4(32, 10, 32, 10), new Vector4(48, 9, 48, 9) };
            var current = ResolutionOptions.s_allowedAspectRatios;
            int n = current?.Length ?? 0;
            var updated = new Il2CppStructArray<Vector4>(n + extra.Length);
            for (int i = 0; i < n; i++)
                updated[i] = current[i];
            for (int i = 0; i < extra.Length; i++)
                updated[n + i] = extra[i];
            ResolutionOptions.s_allowedAspectRatios = updated;
            WidescreenFix.Log.LogInfo($"Added 32:9, 32:10 and 48:9 to the resolution list ({n} -> {updated.Length} aspect ratios).");
        }
        catch (Exception ex)
        {
            WidescreenFix.Log.LogError($"Could not extend allowed aspect ratios: {ex}");
        }
    }
}

public class WidescreenFixBehaviour : MonoBehaviour
{
    bool _resolutionChecked;
    int _frames;

    void Update()
    {
        // Let the game apply its saved resolution first.
        if (_resolutionChecked || ++_frames < 120)
            return;
        _resolutionChecked = true;

        int dw = Display.main.systemWidth, dh = Display.main.systemHeight;
        WidescreenFix.Log.LogInfo($"Screen {Screen.width}x{Screen.height}, desktop {dw}x{dh}, mode {Screen.fullScreenMode}");
        if (!WidescreenFix.UseDesktopResolution.Value || Screen.fullScreenMode == FullScreenMode.Windowed || dh <= 0 || Screen.height <= 0)
            return;

        float desktopAspect = (float)dw / dh, screenAspect = (float)Screen.width / Screen.height;
        if (desktopAspect - screenAspect > 0.01f)
        {
            WidescreenFix.Log.LogInfo($"Switching to desktop resolution {dw}x{dh}");
            Screen.SetResolution(dw, dh, Screen.fullScreenMode);
        }
    }
}
