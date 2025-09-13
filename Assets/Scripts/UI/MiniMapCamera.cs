using UnityEngine;

namespace UI
{
    /// <summary>Top-down orthographic mini-map camera that follows and optionally rotates with the target.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class MiniMapCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(1f)] private float height = 20f;     // meters above player
        [SerializeField, Min(1f)] private float orthoSize = 15f;  // mini-map zoom
        [SerializeField] private bool rotateWithTarget = true;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = orthoSize;
        }

        private void LateUpdate()
        {
            if (!target) return;

            var tPos = target.position;
            transform.position = new Vector3(tPos.x, height, tPos.z);
            transform.rotation = rotateWithTarget && target.hasChanged
                ? Quaternion.Euler(90f, target.eulerAngles.y, 0f)      // look straight down, yaw = player yaw
                : Quaternion.Euler(90f, 0f, 0f);
        }

        public void SetTarget(Transform t) => target = t;
    }
}