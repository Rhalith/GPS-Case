using UnityEngine;
using UnityEngine.AI;

namespace Navigation
{
    /// <summary>Snaps to NavMesh only when accuracy is in a reasonable band and the path is close.</summary>
    public sealed class NavMeshSnapper : MonoBehaviour, IPositionSnapper
    {
        [Header("Accuracy Band (meters)")]
        [Tooltip("Min accuracy to allow snapping (e.g., 8m). Below this we trust GPS and don't snap.")]
        [SerializeField, Min(0f)] private float minAccuracyToSnap = 8f;
        [Tooltip("Max accuracy to allow snapping (e.g., 20m). Above this it's too noisy; don't snap.")]
        [SerializeField, Min(0f)] private float maxAccuracyToSnap = 20f;

        [Header("Proximity")]
        [Tooltip("Max distance from desired to accept a snap (meters).")]
        [SerializeField, Min(0f)] private float maxSnapDistanceMeters = 5f;

        [Header("NavMesh")]
        [SerializeField] private int areaMask = NavMesh.AllAreas;

        public bool TrySnap(in Vector3 desiredWorldPos, double accuracyMeters, out Vector3 snappedWorldPos)
        {
            snappedWorldPos = desiredWorldPos;

            float acc = (float)accuracyMeters;
            if (acc < minAccuracyToSnap || acc > maxAccuracyToSnap)
                return false;

            if (NavMesh.SamplePosition(desiredWorldPos, out var hit, maxSnapDistanceMeters, areaMask))
            {
                // Keep ground Y? The controller clamps Y anyway; we return full position.
                snappedWorldPos = hit.position;
                return true;
            }
            return false;
        }
    }
}