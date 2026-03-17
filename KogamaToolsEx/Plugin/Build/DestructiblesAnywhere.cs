using HarmonyLib;

namespace KogamaToolsEx.Plugin.Build
{
    // Lets you use destructible materials in any model
    [HarmonyPatch]
    internal static class DestructiblesAnywhere
    {
        [HarmonyPatch(typeof(MVMaterial), nameof(MVMaterial.IsAvailable), MethodType.Getter)]
        [HarmonyPostfix]
        private static void MVMaterials_get_IsAvailable(ref bool __result)
        {
            __result = true;
        }
    }
}
