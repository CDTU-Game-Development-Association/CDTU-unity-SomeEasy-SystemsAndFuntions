using UnityEngine;

namespace CDTU.Utils
{
    /// <summary>
    /// Converts a caller-provided screen position to a world-space XY plane.
    /// </summary>
    public static class MouseHelper2D
    {
        public static bool TryGetWorldPosition(
            Vector2 screenPosition,
            out Vector2 worldPosition,
            Camera camera = null,
            float planeZ = 0f)
        {
            if (float.IsNaN(planeZ) || float.IsInfinity(planeZ))
                throw new System.ArgumentOutOfRangeException(nameof(planeZ));

            camera = camera != null ? camera : Camera.main;
            if (camera == null)
            {
                worldPosition = default;
                return false;
            }

            var plane = new Plane(Vector3.forward, new Vector3(0f, 0f, planeZ));
            var ray = camera.ScreenPointToRay(screenPosition);
            if (!plane.Raycast(ray, out var distance))
            {
                worldPosition = default;
                return false;
            }

            var point = ray.GetPoint(distance);
            worldPosition = new Vector2(point.x, point.y);
            return true;
        }

        public static Vector2 GetDirectionFrom(Vector2 origin, Vector2 target)
        {
            return (target - origin).normalized;
        }

        public static bool IsPointInRect(Vector2 worldPosition, Rect worldRect)
        {
            return worldRect.Contains(worldPosition);
        }
    }

    /// <summary>
    /// Explicit drag state that can be owned per pointer or interaction.
    /// </summary>
    public sealed class DragTracker2D
    {
        private Vector2 _lastPosition;
        private bool _hasPosition;

        public Vector2 Update(Vector2 currentPosition)
        {
            if (!_hasPosition)
            {
                _lastPosition = currentPosition;
                _hasPosition = true;
                return Vector2.zero;
            }

            var delta = currentPosition - _lastPosition;
            _lastPosition = currentPosition;
            return delta;
        }

        public void Reset()
        {
            _hasPosition = false;
        }
    }
}
