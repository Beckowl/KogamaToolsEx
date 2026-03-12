namespace KogamaToolsEx.Plugin.Build.CustomEditorStates
{
    internal static class FSMExtensions
    {
        public static void PushState(this FSMEntity e, EditorEventEx nextState)
        {
            e.PushState((EditorEvent)nextState);
        }

        public static void PushState(this FSMEntity e, EditorEventEx nextState, EditorEventEx overridePushState)
        {
            e.PushState((EditorEvent)nextState, (EditorEvent)overridePushState);
        }
    }
}
