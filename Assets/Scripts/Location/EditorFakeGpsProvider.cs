using System.Collections;
using UnityEngine;

namespace Location
{
    /// <summary>Simple loop that advances a fake GPS point every tick (editor testing).</summary>
    public sealed class EditorFakeGpsProvider : IGpsProvider
    {
        private LocationData _cur;
        private bool _ready;

        // configurable "geo" step per tick (~3m north each update by default)
        private readonly double _stepLatDeg;
        private readonly double _stepLonDeg;

        public EditorFakeGpsProvider(double startLat = 38.4237, double startLon = 27.1428, double metersPerStepNorth = 3.0, double metersPerStepEast = 0.0)
        {
            _cur = new LocationData(startLat, startLon, 2.0, 0.0);

            // 1 deg latitude ≈ 111_132 m; 1 deg longitude ≈ 111_320 * cos(lat)
            _stepLatDeg = metersPerStepNorth / 111_132.0;
            _stepLonDeg = metersPerStepEast  / (111_320.0 * Mathf.Cos((float)(startLat * Mathf.Deg2Rad)));
        }

        public IEnumerator Initialize()
        {
            _ready = true;
            yield break;
        }

        public bool TryGetLatest(out LocationData data)
        {
            // Advance a tiny bit every call to simulate movement
            var nextLat = _cur.Latitude + _stepLatDeg;
            var nextLon = _cur.Longitude + _stepLonDeg;
            _cur = new LocationData(nextLat, nextLon, 20, Time.timeAsDouble);
            data = _cur;
            return _ready;
        }

        public void Shutdown()
        {
            _ready = false;
        }
    }
}