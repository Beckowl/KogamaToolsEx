using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using KogamaToolsEx.Plugin.Misc;
using KogamaToolsEx.Hook;
using ImGuiNET;

namespace KogamaToolsEx;

[BepInPlugin(PluginMeta.GUID, PluginMeta.NAME, PluginMeta.VERSION)]
public class KogamaTools : BasePlugin
{
    private readonly Harmony harmony = new(PluginMeta.GUID);
    private ImGuiHook imguiHook = new();
    internal static ManualLogSource Logger;

    public override void Load()
    {
        Logger = Log;
        Logger.LogInfo($"Plugin {PluginMeta.GUID} is loaded!");

        harmony.PatchAll();
        imguiHook.Initialize();

        // test
        imguiHook.OnRender += () => ImGui.ShowDemoWindow();
    }

    public override bool Unload()
    {
        Logger.LogInfo("Unloading");

        imguiHook.Dispose();

        return false; // ?
    }

    [InvokeOnInit]
    private static void DoGreeting()
    {
        const string msg =
            "<color=cyan>Welcome to {0} v{1}!</color>\n\n" +
            "The quick brown fox jumps over the lazy dog.";

        TextCommand.NotifyUser(string.Format(msg, PluginMeta.NAME, PluginMeta.VERSION));
    }
}