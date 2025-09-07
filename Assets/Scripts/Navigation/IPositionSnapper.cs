using UnityEngine;

namespace Navigation
{
    /// <summary>Strategy interface for snapping a desired world position to a valid position.</summary>
    public interface IPositionSnapper
    {
        /// <summary>
        /// If snapping rules are met, returns a snapped position and true; otherwise false.
        /// </summary>
        bool TrySnap(in Vector3 desiredWorldPos, double accuracyMeters, out Vector3 snappedWorldPos);
    }
}