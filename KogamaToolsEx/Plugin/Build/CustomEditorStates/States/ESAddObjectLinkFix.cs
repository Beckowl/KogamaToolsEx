using KogamaToolsEx.Helpers;
using MV.WorldObject;

namespace KogamaToolsEx.Plugin.Build.CustomEditorStates.States
{
    internal class ESAddObjectLinkFix : ESStateCustomBase
    {
        private WorldObjectClientRef woRef;
        private ObjectLink tempLink; 

        public override void Enter(EditorStateMachine e)
        {
            var wo = Pick();

            if (wo != null && wo.HasObjectConnector)
            {
                tempLink = new();

                tempLink.objectConnectorWOID = wo.id;
                MVGameControllerBase.MainCameraManager.LineDrawManager.SetTempObjectLink(tempLink);
                woRef = MVGameControllerBase.WOCM.GetWorldObjectClientRef(wo.id);
            }
            else
            {
                e.PopState();
            }

        }

        public override void Execute(EditorStateMachine e)
        {
            var connectorWo = woRef.WorldObjectClient;

            if (connectorWo == null)
            {
                e.PopState();
                return;
            }

            if (MVInputWrapper.GetBooleanControlUp(KogamaControls.PointerSelect))
            { 
                var wo = Pick();

                if (wo == null)
                {
                    e.PopState(); 
                    return;
                }

                if (wo.Id != connectorWo.Id && connectorWo.ValidateObjectLinkTarget(wo))
                {
                    tempLink.objectWOID = wo.id;
                    MVGameControllerBase.OperationRequests.AddObjectLink(tempLink);
                }

                e.DeSelectAll();
                LeaveAddLink(e);
            }
        }

        private MVWorldObjectClient Pick()
        {
            VoxelHit vhit = new();

            if (ObjectPicker.Pick(ref vhit))
                return MVGameControllerBase.WOCM.GetWorldObjectClient(vhit.woId);
            else
                return null;
        }

        private void LeaveAddLink(EditorStateMachine e)
        {
            if (e.ParentGroupID == MVGameControllerBase.WOCM.RootGroup.Id)
                e.Event = new Il2CppEnum<EditorEvent>(EditorEvent.ESTerrainEdit);
            else
                e.PopState();
        }

        public override void Exit(EditorStateMachine e)
        {
            MVGameControllerBase.MainCameraManager.LineDrawManager.SetTempObjectLink(null);
        }
    }
}
