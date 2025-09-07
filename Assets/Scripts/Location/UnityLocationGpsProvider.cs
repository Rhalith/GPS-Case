using System.Collections;
using UnityEngine;

namespace Location
{
    /// <summary>Uses Unity's LocationService (iOS/Android; on desktop may be emulated; WebGL usually unsupported).</summary>
    public sealed class UnityLocationGpsProvider : IGpsProvider
    {
        private bool _ready;
        private LocationData _last;

        public IEnumerator Initialize()
        {
            if (!Input.location.isEnabledByUser)
            {
                // user disabled location permission
                _ready = false;
                yield break;
            }

            Input.location.Start(desiredAccuracyInMeters: 5f, updateDistanceInMeters: 0.5f);

            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                maxWait--;
                yield return null;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                _ready = false;
                yield break;
            }

            _ready = true;
            var d = Input.location.lastData;
            _last = new LocationData(d.latitude, d.longitude, d.horizontalAccuracy, d.timestamp);
        }

        public bool TryGetLatest(out LocationData data)
        {
            if (!_ready || Input.location.status != LocationServiceStatus.Running)
            {
                data = default;
                return false;
            }

            var d = Input.location.lastData;
            data = new LocationData(d.latitude, d.longitude, d.horizontalAccuracy, d.timestamp);
            _last = data;
            return true;
        }

        public void Shutdown()
        {
            if (Input.location.status == LocationServiceStatus.Running)
                Input.location.Stop();
            _ready = false;
        }
    }
}