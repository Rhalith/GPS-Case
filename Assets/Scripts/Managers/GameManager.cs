using UnityEngine;

namespace Managers
{
    [DefaultExecutionOrder(-100)] // make sure we initialize early
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private GameObject playerPrefab; 

        [Header("Options")]
        [SerializeField] private bool spawnOnStart = true;

        public Transform StartPoint => startPoint;
        public GameObject Player { get; private set; }

        private void Awake()
        {
            if (Instance && !Equals(Instance, this))
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (spawnOnStart)
                SpawnPlayerAtStartPoint();
        }

        private void SpawnPlayerAtStartPoint()
        {
            if (!playerPrefab || !startPoint)
            {
                Debug.LogError("[GameManager] Missing playerPrefab or startPoint.");
                return;
            }

            if (Player) Destroy(Player);

            Player = Instantiate(playerPrefab, startPoint.position, startPoint.rotation);
        }

        /// <summary>Utility for later phases (GPS mapping).</summary>
        public void TeleportPlayer(Vector3 worldPosition, Quaternion? rotation = null)
        {
            if (!Player) return;
            var rot = rotation ?? Player.transform.rotation;
            Player.transform.SetPositionAndRotation(worldPosition, rot);
        }
    }
}