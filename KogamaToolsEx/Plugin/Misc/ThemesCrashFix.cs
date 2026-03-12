using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KogamaToolsEx.Plugin.Misc
{
    [HarmonyPatch]
    // shoutouts to Nightus!!
    internal class ThemesCrashFix
    {
        [HarmonyPatch(typeof(Theme), "Initialize", [typeof(int)])]
        [HarmonyPrefix]
        private static void ThemeInitializeIntPrefix(int woid, ref bool __runOriginal)
        {
            var wo = MVGameControllerBase.WOCM.GetWorldObject(woid) as MVWorldObjectClient;

            if (wo != null)
            {
                __runOriginal = true;
                return;
            }

            // todo: recover??? i'm pretty sure we could just request a new theme world object
            // right??
            wo.Destroy();
            __runOriginal = false;
        }
    }
}
