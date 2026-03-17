using HarmonyLib;

namespace KogamaToolsEx.Plugin.Build
{
    [HarmonyPatch]
    internal static class NoBuildLimit
    {
        // TODO: Constrain avatars

        [HarmonyPatch(typeof(ConstraintVisualizer), "Init")]
        [HarmonyPrefix]
        private static void ConstraintVisualizer_Init_Prefix(ref bool __runOriginal)
        {
            __runOriginal = false;
        }

        [HarmonyPatch(typeof(ModelingDynamicBoxConstraint), "CanAddCubeAt")]
        [HarmonyPatch(typeof(ModelingBoxCountConstraint), "CanAddCubeAt")]
        [HarmonyPatch(typeof(ModelingBoxCountConstraint), "CanRemoveCubeAt")]
        [HarmonyPostfix]
        private static void CanAddRemoveCubeAt_Postfix(ref bool __result)
        {
            __result = true;
        }
    }
}
