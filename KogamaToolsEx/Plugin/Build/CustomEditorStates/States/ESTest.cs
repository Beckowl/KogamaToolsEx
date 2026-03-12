namespace KogamaToolsEx.Plugin.Build.CustomEditorStates.States
{
    internal class ESTest : ESStateCustomBase
    {
        public override void Enter(EditorStateMachine e)
        {
            KogamaTools.Logger.LogInfo("entering ESTest");
        }

        public override void Execute(EditorStateMachine e)
        {
            KogamaTools.Logger.LogInfo("ESTest Execute");
            e.PopState();
        }

        public override void Exit(EditorStateMachine e)
        {
            KogamaTools.Logger.LogInfo("ESTest Exit");
        }
    }
}
