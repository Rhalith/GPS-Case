using System.Collections;
using System.Runtime.InteropServices;

namespace Location
{
    /// <summary>WebGL provider using navigator.geolocation via .jslib.</summary>
    public sealed class WebGlGpsProvider : IGpsProvider
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void WebGLGeo_Start(int enableHighAccuracy, int timeoutMs, int maximumAgeMs);
        [DllImport("__Internal")] private static extern void WebGLGeo_Stop();
        [DllImport("__Internal")] private static extern int    WebGLGeo_GetHasFix();
        [DllImport("__Internal")] private static extern double WebGLGeo_GetLat();
        [DllImport("__Internal")] private static extern double WebGLGeo_GetLon();
        [DllImport("__Internal")] private static extern double WebGLGeo_GetAcc();
        [DllImport("__Internal")] private static extern double WebGLGeo_GetTime();
#endif

        private bool _ready;

        public IEnumerator Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Request permission; browsers will prompt the user here.
            WebGLGeo_Start(enableHighAccuracy: 1, timeoutMs: 10000, maximumAgeMs: 0);
            // No lengthy init; allow first poll to check for fix.
            _ready = true;
#else
            _ready = false; // should never be used outside WebGL
#endif
            yield break;
        }

        public bool TryGetLatest(out LocationData data)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!_ready || WebGLGeo_GetHasFix() == 0)
            {
                data = default;
                return false;
            }
            data = new LocationData(WebGLGeo_GetLat(), WebGLGeo_GetLon(), WebGLGeo_GetAcc(), WebGLGeo_GetTime());
            return true;
#else
            data = default;
            return false;
#endif
        }

        public void Shutdown()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLGeo_Stop();
#endif
            _ready = false;
        }
    }
}