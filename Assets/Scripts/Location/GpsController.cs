using System.Collections;
using UnityEngine;

namespace Location
{
    /// <summary>Maps GPS changes to world-space meters and teleports the player every interval.</summary>
    public sealed class GpsController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform player;      // assign spawned player root
        [SerializeField] private Transform startPoint;  // same StartPoint used by GameManager

        [Header("UI (optional)")]
        [SerializeField] private UI.GpsHud hud; 
        
        [Header("Provider")]
        [SerializeField] private ProviderType provider = ProviderType.EditorFake;

        [Header("Options")]
        [Tooltip("Seconds between polls.")]
        [SerializeField, Min(1f)] private float pollSeconds = 3f;
        [Tooltip("Swap X/Z axes if your model's forward is different.")]
        [SerializeField] private bool swapXZ;

        private IGpsProvider _gps;
        private bool _running;
        private bool _hasOrigin;
        private LocationData _origin;

        // can be private
        private enum ProviderType { UnityLocationService, EditorFake }

        private void Awake()
        {
            if (!player)     Debug.LogWarning("[GpsController] Player not assigned.");
            if (!startPoint) Debug.LogWarning("[GpsController] StartPoint not assigned.");
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            _gps = provider == ProviderType.UnityLocationService
                ? new UnityLocationGpsProvider()
                : new EditorFakeGpsProvider();
#else
            _gps = provider == ProviderType.UnityLocationService
                ? new UnityLocationGpsProvider()
                : new EditorFakeGpsProvider();
#endif
            hud?.SetStatus("Initializing…"); 
            StartCoroutine(RunRoutine());
        }

        private void OnDisable()
        {
            _gps?.Shutdown();
            _running = false;
            _hasOrigin = false;
            hud?.SetStatus("Stopped");  
        }

        private IEnumerator RunRoutine()
        {
            // initialize provider
            yield return StartCoroutine(_gps.Initialize());
            
            // If provider couldn't start, keep HUD honest and bail out.
            LocationData tmp;
            if (!_gps.TryGetLatest(out tmp))
            {
                hud?.SetStatus("Location unavailable");
                yield break;
            }
            _running = true;
            hud?.SetStatus("Running"); 
            
            // cache wait instruction; recreate only when interval changes
            float lastInterval = Mathf.Max(1f, pollSeconds);
            WaitForSeconds wait = new WaitForSeconds(lastInterval);

            while (_running && isActiveAndEnabled)
            {
                if (_gps.TryGetLatest(out var sample))
                {
                    hud?.SetSample(sample);
                    if (!_hasOrigin)
                    {
                        _origin = sample;
                        _hasOrigin = true;

                        if (player && startPoint)
                            player.position = startPoint.position; // snap to start immediately
                    }
                    else
                    {
                        // compute delta (meters) origin -> current
                        Vector2 deltaMeters = GeoDeltaMeters(_origin, sample);
                        Vector3 offset = swapXZ
                            ? new Vector3(deltaMeters.y, 0f, deltaMeters.x)   // swap X/Z if needed
                            : new Vector3(deltaMeters.x, 0f, deltaMeters.y);

                        if (player && startPoint)
                            player.position = startPoint.position + offset;   // teleport
                    }
                }

                // if inspector changed pollSeconds at runtime, refresh the wait object
                if (!Mathf.Approximately(lastInterval, pollSeconds))
                {
                    lastInterval = Mathf.Max(1f, pollSeconds);
                    wait = new WaitForSeconds(lastInterval);
                }

                yield return wait;
            }
        }

        /// <summary>Returns (east, north) in meters between two GPS points.</summary>
        private static Vector2 GeoDeltaMeters(in LocationData a, in LocationData b)
        {
            double meanLatRad = ((a.Latitude + b.Latitude) * 0.5) * Mathf.Deg2Rad;
            double dLat = (b.Latitude  - a.Latitude)  * Mathf.Deg2Rad;
            double dLon = (b.Longitude - a.Longitude) * Mathf.Deg2Rad;

            const double earthRadius = 6378137.0; // meters (WGS84 a)
            double east  = dLon * System.Math.Cos(meanLatRad) * earthRadius;
            double north = dLat * earthRadius;

            return new Vector2((float)east, (float)north);
        }
    }
}
