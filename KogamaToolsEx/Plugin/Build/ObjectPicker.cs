using UnityEngine;

namespace KogamaToolsEx.Plugin.Build
{
    internal static class ObjectPicker
    {
        public static LinkObjectBase PickLink(ref VoxelHit hit)
        {
            if (!MVGameControllerBase.MainCameraManager.IsLogicRendered)
                return null;

            float dist = Pick(ref hit) ? hit.distance : float.PositiveInfinity;

            Ray ray = MVGameControllerBase.MainCameraManager.MainCamera.ScreenPointToRay(MVInputWrapper.GetPointerPosition());
            int layerMask = 1 << LayerMask.NameToLayer("Logic") | 1 << LayerMask.NameToLayer("LogicSelected");

            Physics.Raycast(ray, out var raycastHit, float.PositiveInfinity, layerMask);

            if (raycastHit.collider != null && raycastHit.distance < dist)
            {
                hit.point = raycastHit.point;
                return raycastHit.collider.gameObject.GetComponentInChildren<LinkObjectBase>();
            }

            return null;
        }

        internal static bool Pick(ref VoxelHit vhit, Il2CppSystem.Collections.Generic.HashSet<int> ignoreWoIds = null!, int layerMask = -262149)
        {
            var mousePos = MVInputWrapper.GetPointerPosition();
            var ray = EditModeObjectPicker.MainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y));

            float drawPlaneDist = 0f;
            bool drawPlaneHit = IsDrawPlaneHit(ref drawPlaneDist, ray);

            var hits = CollisionDetection.MVHitAll(ray, float.PositiveInfinity, ignoreWoIds, layerMask);

            if (hits.Count == 0)
                return false;

            return TryGetClosestHit(ray, hits, drawPlaneHit, drawPlaneDist, ref vhit);
        }

        private static bool IsDrawPlaneHit(ref float drawPlaneDist, Ray ray)
        {
            if (!DrawPlane.IsDrawPlaneActive)
                return false;

            Vector3 planeHit = Vector3.zero;

            if (DrawPlane.Pick(ref planeHit))
            {
                drawPlaneDist = (planeHit - ray.origin).magnitude;
                return true;
            }

            return false;
        }

        private static bool TryGetClosestHit(Ray ray, Il2CppSystem.Collections.Generic.List<VoxelHit> hits, bool drawPlaneHit, float drawPlaneDist, ref VoxelHit vhit)
        {
            float maxDist = float.PositiveInfinity;
            bool hit = false;

            foreach (VoxelHit voxelHit in hits)
            {
                if ((!drawPlaneHit || voxelHit.distance < drawPlaneDist) && voxelHit.transform.gameObject.activeInHierarchy)
                {
                    if (voxelHit.distance < maxDist)
                    {
                        maxDist = voxelHit.distance;
                        vhit = voxelHit;
                        hit = true;
                    }
                }
            }

            return hit;
        }
    }
}
