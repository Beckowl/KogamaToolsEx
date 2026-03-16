using HarmonyLib;

namespace KogamaToolsEx.Plugin.Misc
{
    internal class QueryEventListener
    {
        internal static event Action<MVWorldObjectClient, int> OnQueryEvent;
        internal static event Action<MVWorldObjectClient> OnQueryEventLocal;

        [HarmonyPatch(typeof(WorldNetwork), "CreateQueryEvent")]
        [HarmonyPostfix]
        private static void CreateQueryEvent_Postfix(MVWorldObjectClient root, int instigatorActorNumber)
        {
            if (!MVGameControllerBase.IsInitialized)
                return;

            OnQueryEvent?.Invoke(root, instigatorActorNumber);

            if (instigatorActorNumber == MVGameControllerBase.Game.LocalPlayer.ActorNr)
                OnQueryEventLocal?.Invoke(root);
        }
    }
}
