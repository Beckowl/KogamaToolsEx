using HarmonyLib;
using MV.Common;

namespace KogamaToolsEx.Plugin.Misc
{
    [HarmonyPatch]
    internal static class AntiBan
    {
        [HarmonyPatch(typeof(CheatHandling), "Init")]
        [HarmonyPatch(typeof(CheatHandling), "ExecuteBan")]
        [HarmonyPatch(typeof(CheatHandling), "MachineBanDetected")]
        [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Ban", [typeof(int), typeof(MVPlayer), typeof(string)])]
        [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Ban", [typeof(CheatType)])]
        [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Expel")]
        [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Kick")]
        [HarmonyPrefix]
        private static void NoBan(ref bool __runOriginal)
        {
            __runOriginal = false;
        }
    }
}
