using UnityEngine;

namespace CDTU.Utils
{
    /// <summary>
    /// Builds 3D rays from a caller-provided screen position.
    /// </summary>
    public static class MouseHelper3D
    {
        public static bool TryGetRay(Vector2 screenPosition, out Ray ray, Camera camera = null)
        {
            camera = camera != null ? camera : Camera.main;
            if (camera == null)
            {
                ray = default;
                return false;
            }

            ray = camera.ScreenPointToRay(screenPosition);
            return true;
        }

        public static bool RaycastObject(
            Vector2 screenPosition,
            out RaycastHit hit,
            Camera camera = null,
            float maxDistance = 1000f,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (float.IsNaN(maxDistance) || float.IsInfinity(maxDistance) || maxDistance < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(maxDistance));

            if (!TryGetRay(screenPosition, out var ray, camera))
            {
                hit = default;
                return false;
            }

            return Physics.Raycast(ray, out hit, maxDistance, layerMask, triggerInteraction);
        }

        public static bool RaycastToGround(
            Vector2 screenPosition,
            out Vector3 hitPoint,
            Camera camera = null,
            float maxDistance = 1000f,
            int groundMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            if (RaycastObject(
                    screenPosition,
                    out var hit,
                    camera,
                    maxDistance,
                    groundMask,
                    triggerInteraction))
            {
                hitPoint = hit.point;
                return true;
            }

            hitPoint = default;
            return false;
        }

        public static bool RaycastToPlane(
            Vector2 screenPosition,
            Plane plane,
            out Vector3 hitPoint,
            Camera camera = null)
        {
            if (TryGetRay(screenPosition, out var ray, camera) &&
                plane.Raycast(ray, out var distance))
            {
                hitPoint = ray.GetPoint(distance);
                return true;
            }

            hitPoint = default;
            return false;
        }

        public static Vector3 GetFlatDirection(Vector3 origin, Vector3 target)
        {
            var direction = target - origin;
            direction.y = 0f;
            return direction.normalized;
        }
    }
}
