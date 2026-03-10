using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using KogamaToolsEx;
using KogamaToolsEx.Misc;

namespace KogamaToolsX;

[BepInPlugin(PluginMeta.GUID, PluginMeta.NAME, PluginMeta.VERSION)]
public class Plugin : BasePlugin
{
    internal static ManualLogSource Logger;

    private readonly Harmony harmony = new(PluginMeta.GUID);

    public override void Load()
    {
        Logger = base.Log;
        Logger.LogInfo($"Plugin {PluginMeta.GUID} is loaded!");

        harmony.PatchAll();
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