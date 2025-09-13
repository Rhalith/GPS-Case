// using ... (unchanged)
using System.Collections;
using UnityEngine;
using Navigation;

namespace Location
{
    public sealed class GpsController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform startPoint;

        [Header("UI (optional)")]
        [SerializeField] private UI.GpsHud hud;

        [Header("Snapping (optional)")]
        [SerializeField] private MonoBehaviour snapperBehaviour; // NavMeshSnapper, etc.
        private IPositionSnapper _snapper;

        [Header("Update Cadence")]
        [Tooltip("Seconds between position updates (teleport tick).")]
        [SerializeField, Min(1f)] private float pollSeconds = 3f;
        [Tooltip("How often to refresh 'Current Coordinates' on HUD (Hz).")]
        [SerializeField, Min(1f)] private float currentHudHz = 5f;

        [Header("Mapping")]
        [Tooltip("Swap X/Z axes if your model's forward is different.")]
        [SerializeField] private bool swapXZ;

        [Header("Noise Rejection")]
        [Tooltip("If accuracy is better than this (meters), trust the reading.")]
        [SerializeField, Min(0f)] private float trustAccuracyMeters = 8f;
        [Tooltip("If accuracy is worse, only accept movements larger than this (meters).")]
        [SerializeField, Min(0f)] private float minMovementMeters = 10f;
        [Tooltip("Extra guard: movement must also exceed accuracy * multiplier.")]
        [SerializeField, Range(1f, 3f)] private float accuracyMultiplierGate = 1.5f;

        [Header("Smoothing")]
        [SerializeField] private bool smoothAcceptedMoves = true;
        [SerializeField, Range(0.1f, 2f)] private float smoothSeconds = 0.75f;

        private IGpsProvider _gps;
        private bool _running;
        private bool _hasOrigin;
        private LocationData _origin;
        private LocationData _lastUpdated;

        private Vector3 _prevTarget; // last placed target
        private Coroutine _moveRoutine;

        private void Awake()
        {
            if (snapperBehaviour) _snapper = snapperBehaviour as IPositionSnapper;
            if (snapperBehaviour && _snapper == null)
                Debug.LogError("[GpsController] Snapper does not implement IPositionSnapper.");
        }

        private void OnEnable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _gps = new WebGlGpsProvider();
#else
            _gps = new EditorFakeGpsProvider(); // or UnityLocationGpsProvider if you prefer
#endif
            hud?.SetStatus("Initializing…");
            StartCoroutine(RunTickRoutine());
            StartCoroutine(RunCurrentHudRoutine());
        }

        private void OnDisable()
        {
            _gps?.Shutdown();
            _running = false;
            _hasOrigin = false;
            hud?.SetStatus("Stopped");
        }

        private IEnumerator RunTickRoutine()
        {
            yield return StartCoroutine(_gps.Initialize());

            if (!_gps.TryGetLatest(out var first))
            {
                hud?.SetStatus("Location unavailable");
                yield break;
            }

            _running = true;
            _origin = first;
            _hasOrigin = true;
            _lastUpdated = first;
            hud?.SetStatus("Running");

            if (player && startPoint)
            {
                _prevTarget = startPoint.position;
                player.position = _prevTarget;
            }

            float interval = Mathf.Max(1f, pollSeconds);
            var wait = new WaitForSeconds(interval);

            while (_running && isActiveAndEnabled)
            {
                if (_gps.TryGetLatest(out var sample))
                {
                    _lastUpdated = sample;
                    hud?.SetLastUpdated(sample);

                    // meters delta from anchor
                    Vector2 deltaMeters = GeoDeltaMeters(_origin, sample);
                    Vector3 offset = swapXZ
                        ? new Vector3(deltaMeters.y, 0f, deltaMeters.x)
                        : new Vector3(deltaMeters.x, 0f, deltaMeters.y);

                    // desired raw target (keep ground Y from StartPoint)
                    Vector3 target = startPoint ? startPoint.position + offset : offset;

                    // ----- JITTER GATE -----
                    float planarDist = Vector2.Distance(new Vector2(_prevTarget.x, _prevTarget.z),
                        new Vector2(target.x,     target.z));
                    float acc = (float)_lastUpdated.AccuracyMeters;

                    bool accuracyGood   = acc <= trustAccuracyMeters;
                    bool bigEnoughMove  = planarDist > Mathf.Max(minMovementMeters, acc * accuracyMultiplierGate);

                    bool acceptMove = accuracyGood || bigEnoughMove;
                    
                    if (hud)
                    {
                        if (acceptMove)
                        {
                            if (accuracyGood)
                                hud.SetDecision($"Accepted: acc {acc:F1} ≤ {trustAccuracyMeters:F1}");
                            else
                                hud.SetDecision($"Accepted: move {planarDist:F1} > max({minMovementMeters:F1}, {acc*accuracyMultiplierGate:F1})");
                        }
                        else
                        {
                            hud.SetDecision($"Rejected (jitter): move {planarDist:F1}, acc {acc:F1}");
                        }
                    }

                    if (acceptMove)
                    {
                        bool snappedNow = false;
                        // ----- OPTIONAL SNAPPING (XZ only; keep Y) -----
                        if (_snapper != null && _snapper.TrySnap(target, _lastUpdated.AccuracyMeters, out var snapped))
                        {
                            target = new Vector3(snapped.x, target.y, snapped.z);
                            snappedNow = true;
                        }
                        
                        
                        if (snappedNow && hud != null)
                            hud.SetDecision($"{(accuracyGood ? "Accepted" : "Accepted")} + Snapped");

                        // Face direction (flat yaw)
                        Vector3 dir = target - _prevTarget; dir.y = 0f;
                        if (dir.sqrMagnitude > 0.0001f && player)
                            player.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);


                        // Move
                        if (player)
                        {
                            if (smoothAcceptedMoves)
                            {
                                if (_moveRoutine != null) StopCoroutine(_moveRoutine);
                                _moveRoutine = StartCoroutine(SmoothMove(player, target, smoothSeconds));
                            }
                            else
                            {
                                player.position = target;
                            }
                        }

                        _prevTarget = target;
                    }
                    // else: ignore this noisy micro-jump
                }

                if (!Mathf.Approximately(interval, pollSeconds))
                {
                    interval = Mathf.Max(1f, pollSeconds);
                    wait = new WaitForSeconds(interval);
                }

                yield return wait;
            }
        }

        private IEnumerator RunCurrentHudRoutine()
        {
            float step = 1f / Mathf.Max(1f, currentHudHz);
            var wait = new WaitForSeconds(step);

            while (isActiveAndEnabled)
            {
                if (_gps != null && _gps.TryGetLatest(out var cur))
                    hud?.SetCurrent(cur);
                yield return wait;
            }
        }

        private IEnumerator SmoothMove(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.position;
            float elapsed = 0f;
            seconds = Mathf.Max(0.1f, seconds);
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / seconds);
                t.position = Vector3.Lerp(from, to, k);
                yield return null;
            }
            t.position = to;
        }

        public void ResetToStartAndReanchor()
        {
            if (_gps != null && _gps.TryGetLatest(out var now))
            {
                _origin = now;
                _hasOrigin = true;
                _lastUpdated = now;
                hud?.SetLastUpdated(now);
                hud?.SetCurrent(now);
            }
            if (player && startPoint)
            {
                if (_moveRoutine != null) StopCoroutine(_moveRoutine);
                _prevTarget = startPoint.position;
                player.position = _prevTarget;
            }
        }

        private static Vector2 GeoDeltaMeters(in LocationData a, in LocationData b)
        {
            double meanLatRad = ((a.Latitude + b.Latitude) * 0.5) * Mathf.Deg2Rad;
            double dLat = (b.Latitude - a.Latitude) * Mathf.Deg2Rad;
            double dLon = (b.Longitude - a.Longitude) * Mathf.Deg2Rad;

            const double EarthRadius = 6378137.0;
            double east = dLon * System.Math.Cos(meanLatRad) * EarthRadius;
            double north = dLat * EarthRadius;
            return new Vector2((float)east, (float)north);
        }
    }
}
