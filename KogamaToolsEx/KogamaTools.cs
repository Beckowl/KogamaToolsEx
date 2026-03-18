using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using KogamaToolsEx.Hook;
using ImGuiNET;

namespace KogamaToolsEx;

[BepInPlugin(PluginMeta.GUID, PluginMeta.NAME, PluginMeta.VERSION)]
public class KogamaTools : BasePlugin
{
    private readonly Harmony harmony = new(PluginMeta.GUID);
    internal static ManualLogSource Logger;

    public override void Load()
    {
        Logger = Log;
        Logger.LogInfo($"Plugin {PluginMeta.GUID} is loaded!");

        harmony.PatchAll();

        ImGuiHook.Initialize();
        ImGuiHook.OnRender += () => ImGui.ShowDemoWindow();
    }

    public override bool Unload()
    {
        Logger.LogInfo("Unloading");

        ImGuiHook.Shutdown();

        return false; // ?
    }
}