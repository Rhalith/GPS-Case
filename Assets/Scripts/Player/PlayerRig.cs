using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerRig : MonoBehaviour
    {
        [Header("Dimensions")]
        [Tooltip("Target player height in meters.")]
        [SerializeField] private float height = 1.8f;
        [SerializeField] private float radius = 0.3f;

        [Header("Camera")]
        [SerializeField] private Camera fpsCamera;
        [Range(0.5f, 0.8f)]
        [SerializeField] private float eyeHeightRatio = 0.65f;
        
        [SerializeField] private CapsuleCollider capsuleCollider;

        private void Awake()
        {
            if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider>();
            if (!fpsCamera)
            {
                var camGO = new GameObject("FPS_Camera");
                camGO.transform.SetParent(transform, false);
                fpsCamera = camGO.AddComponent<Camera>();
            }
            ApplyDimensions();
        }

        private void OnValidate()
        {
            if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider>();
            ApplyDimensions();
        }

        private void ApplyDimensions()
        {
            if (!capsuleCollider) return;
            
            capsuleCollider.direction = 1;
            capsuleCollider.height = height;
            capsuleCollider.radius = radius;
            capsuleCollider.center = new Vector3(0f, height * 0.5f, 0f);

            if (fpsCamera)
            {
                var eyeY = height * eyeHeightRatio;
                fpsCamera.transform.localPosition = new Vector3(0f, eyeY, 0f);
                fpsCamera.transform.localRotation = Quaternion.identity;
            }
        }
    }
}