using UnityEngine;
using UnityEngine.AI;

namespace Navigation
{
    /// <summary>Snaps to the nearest point on the NavMesh when accuracy is poor and the path is close.</summary>
    public sealed class NavMeshSnapper : MonoBehaviour, IPositionSnapper
    {
        [Header("Rules")]
        [Tooltip("When GPS accuracy (meters) is worse than this, allow snapping.")]
        [SerializeField, Min(0f)] private float accuracyThresholdMeters = 5f;

        [Tooltip("Max search distance from desired to find a NavMesh point (meters).")]
        [SerializeField, Min(0f)] private float maxSnapDistanceMeters = 3f;

        [Tooltip("NavMesh area mask. -1 = AllAreas")]
        [SerializeField] private int areaMask = NavMesh.AllAreas;

        public bool TrySnap(in Vector3 desiredWorldPos, double accuracyMeters, out Vector3 snappedWorldPos)
        {
            snappedWorldPos = desiredWorldPos;

            if (!(accuracyMeters > accuracyThresholdMeters))
                return false;

            if (NavMesh.SamplePosition(desiredWorldPos, out var hit, maxSnapDistanceMeters, areaMask))
            {
                snappedWorldPos = hit.position;
                return true;
            }

            return false;
        }
    }
}