namespace KogamaToolsEx.Plugin.Build.CustomEditorStates
{
    public abstract class ESStateCustomBase
    {
        public virtual void Enter(EditorStateMachine esm) { }
        public virtual void Execute(EditorStateMachine e) { }

        public virtual void Exit(EditorStateMachine esm) { }

        public void Enter(FSMEntity e)
        {
            this.Enter(e.Cast<EditorStateMachine>());
        }

        public void Execute(FSMEntity e)
        {
            this.Execute(e.Cast<EditorStateMachine>());
        }

        public void Exit(FSMEntity e)
        {
            this.Exit(e.Cast<EditorStateMachine>());
        }
    }
}
