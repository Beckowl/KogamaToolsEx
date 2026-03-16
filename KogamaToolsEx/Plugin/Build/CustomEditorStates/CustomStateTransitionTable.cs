using KogamaToolsEx.Plugin.Build.CustomEditorStates.States;

namespace KogamaToolsEx.Plugin.Build.CustomEditorStates
{
    internal class CustomStateTransitionTable
    {
        private Dictionary<EditorEventEx, ESStateCustomBase> table = new();

        public CustomStateTransitionTable()
        {
            AddState(EditorEventEx.ESAddLink, new ESAddLinkFix());
            AddState(EditorEventEx.ESAddObjectLink, new ESAddObjectLinkFix());
        }

        public bool GetState(EditorEventEx e, out ESStateCustomBase state)
        {
            return table.TryGetValue(e, out state);
        }

        protected void AddState(EditorEventEx e, ESStateCustomBase state)
        {
            table.Add(e, state);
        }
    }
}
