using KogamaToolsEx.Helpers;
using MV.WorldObject;
using UnityEngine;

namespace KogamaToolsEx.Plugin.Build.CustomEditorStates.States
{
    internal class ESAddLinkFix : ESStateCustomBase
    {
        private Link tempLink;
        private WorldObjectClientRef woRef;
        private SelectedConnector sourceConnectorType;

        private Material materialToPulse;
        private Color baseColor;
        public float PulseSpeed = 5f;

        public override void Enter(EditorStateMachine e)
        {
            PickResult pick = Pick();

            if (pick.ConnectorType != SelectedConnector.None)
            {
                InitPulsing(pick.ConnectorType);
                tempLink = new Link();
                SetLinkEndpoint(pick.ConnectorType, pick.Wo.Id);
                sourceConnectorType = pick.ConnectorType;

                pick.Wo.HighlightConnector(true);
                MVGameControllerBase.MainCameraManager.LineDrawManager.SetTempLink(tempLink);
                woRef = MVGameControllerBase.WOCM.GetWorldObjectClientRef(pick.Wo.Id);
            }
            else
            {
                e.PopState();
            }
        }

        public override void Execute(EditorStateMachine e)
        {
            if (woRef.WorldObjectClient == null)
            {
                LeaveAddLink(e);
                return;
            }

            DoPulsing();

            if (MVInputWrapper.GetBooleanControlUp(KogamaControls.PointerSelect))
            {
                PickResult pick = Pick();

                if (pick.ConnectorType != SelectedConnector.None && pick.ConnectorType != sourceConnectorType)
                {
                    SetLinkEndpoint(pick.ConnectorType, pick.Wo.Id);
                    MVGameControllerBase.OperationRequests.AddLink(tempLink);
                }

                e.DeSelectAll();
                LeaveAddLink(e);
            }
        }

        private readonly record struct PickResult(SelectedConnector ConnectorType, MVWorldObjectClient Wo);

        private PickResult Pick()
        {
            VoxelHit vhit = new VoxelHit();

            if (ObjectPicker.Pick(ref vhit))
            {
                var wo = MVGameControllerBase.WOCM.GetWorldObjectClient(vhit.woId);

                if (wo != null)
                {
                    var cursorPos = MVInputWrapper.GetPointerPosition();

                    if (wo.HasInputConnector && wo.IsPointOverInputConnector(cursorPos))
                        return new PickResult(SelectedConnector.Input, wo);

                    if (wo.HasOutputConnector && wo.IsPointOverOutputConnector(cursorPos))
                        return new PickResult(SelectedConnector.Output, wo);
                }
            }

            return new PickResult(SelectedConnector.None, null);
        }

        private void SetLinkEndpoint(SelectedConnector connectorType, int woid)
        {
            if (connectorType == SelectedConnector.Input)
                tempLink.inputWOID = woid;
            else if (connectorType == SelectedConnector.Output)
                tempLink.outputWOID = woid;
        }

        private void InitPulsing(SelectedConnector connectorType)
        {
            if (connectorType == SelectedConnector.Input)
                materialToPulse = PrefabPool.Instance.LogicCubeConnectorBlueMaterial;
            else if (connectorType == SelectedConnector.Output)
                materialToPulse = PrefabPool.Instance.LogicCubeConnectorRedMaterial;

            baseColor = materialToPulse.color;
        }

        private void DoPulsing()
        {
            float pulse = (Mathf.Sin(Time.time * PulseSpeed) + 1f) * 0.5f;
            materialToPulse.color = Color.Lerp(baseColor * 0.8f, baseColor * 1.2f, pulse);
        }

        private void LeaveAddLink(EditorStateMachine e)
        {
            if (e.ParentGroupID == MVGameControllerBase.WOCM.RootGroup.Id)
                e.Event = new Il2CppEnum<EditorEvent>(EditorEvent.ESTerrainEdit);
            else
                e.PopState();
        }

        public override void Exit(EditorStateMachine esm)
        {
            if (woRef.WorldObjectClient != null)
                woRef.WorldObjectClient.HighlightConnector(false);

            materialToPulse.color = baseColor;
            MVGameControllerBase.MainCameraManager.LineDrawManager.SetTempLink(null);
        }
    }
}
