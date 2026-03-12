using HarmonyLib;
using KogamaToolsEx.Helpers;

namespace KogamaToolsEx.Plugin.Build.CustomEditorStates
{
    [HarmonyPatch]
    internal static class CustomStatesPatch
    {
        private static CustomStateTransitionTable transitionTable = new();

        [HarmonyPatch(typeof(FSMEntity), nameof(FSMEntity.Event), MethodType.Setter)]
        [HarmonyPrefix]
        private static void FSMEntity_Event_Setter_Prefix(FSMEntity __instance, Il2CppSystem.Object value, ref bool __runOriginal)
        {
            if (value == null || value.Pointer == IntPtr.Zero)
            {
                __runOriginal = true;
                return;
            }

            Il2CppEnum<EditorEventEx> evt = new(value.Pointer);

            if (!transitionTable.GetState(evt, out ESStateCustomBase state))
            {
                __runOriginal = true;
                return;
            }

            __runOriginal = false;

            KogamaTools.Logger.LogInfo($"Switching to custom editor state {evt.Value}");

            // this is a recreation of the original method

            var e = __instance;

            if (e.lockState)
                return;

            e.nextEvent = evt;

            if (state != null)
            {
                if (e.currentState != null)
                    e.currentState.Exit(e);

                e.stateName = evt.Value.ToString();
                // we don't set the current state because i'ts a managed object
                e.nextEvent = null;
                e.prevEvent = e.curEvent;
                e.curEvent = evt;
                state.Enter(e);
                e.data.Clear();
            }

            if (e.clearStack)
                e.stateStack.Clear();
            else
                e.clearStack = true;
        }

        private enum StateMethod { Enter, Execute, Exit }

        private static bool HandleCustomState(FSMEntity e, StateMethod method)
        {
            EditorEventEx evt = new Il2CppEnum<EditorEventEx>(e.curEvent.Pointer);

            if (!transitionTable.GetState(evt, out var state))
                return true;

            switch (method)
            {
                case StateMethod.Enter: state.Enter(e); break;
                case StateMethod.Execute: state.Execute(e); break;
                case StateMethod.Exit: state.Exit(e); break;
            }

            return false;
        }

        [HarmonyPatch(typeof(ESStateBase), "Enter", [typeof(FSMEntity)])]
        [HarmonyPrefix]
        private static void ESStateBase_Enter_Prefix(FSMEntity e, ref bool __runOriginal)
        {
            __runOriginal = HandleCustomState(e, StateMethod.Enter);
        }

        [HarmonyPatch(typeof(ESStateBase), "Execute", [typeof(FSMEntity)])]
        [HarmonyPrefix]
        private static void ESStateBase_Execute_Prefix(FSMEntity e, ref bool __runOriginal)
        {
            __runOriginal = HandleCustomState(e, StateMethod.Execute);
        }

        [HarmonyPatch(typeof(ESStateBase), "Exit", [typeof(FSMEntity)])]
        [HarmonyPrefix]
        private static void ESStateBase_Exit_Prefix(FSMEntity e, ref bool __runOriginal)
        {
            __runOriginal = HandleCustomState(e, StateMethod.Exit);
        }
    }
}
