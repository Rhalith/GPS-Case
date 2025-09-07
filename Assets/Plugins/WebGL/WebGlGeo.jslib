mergeInto(LibraryManager.library, {
  WebGLGeo_Start: function (enableHighAccuracy, timeoutMs, maximumAgeMs) {
    if (typeof navigator === 'undefined' || !navigator.geolocation) {
      Module.WebGLGeo_state = { hasFix: 0, lat: 0, lon: 0, acc: 0, time: 0, watchId: -1 };
      return;
    }
    if (!Module.WebGLGeo_state) {
      Module.WebGLGeo_state = { hasFix: 0, lat: 0, lon: 0, acc: 0, time: 0, watchId: -1 };
    }
    var opts = {
      enableHighAccuracy: !!enableHighAccuracy,
      timeout: timeoutMs|0,
      maximumAge: maximumAgeMs|0
    };
    try {
      Module.WebGLGeo_state.watchId = navigator.geolocation.watchPosition(function (pos) {
        var c = pos.coords;
        Module.WebGLGeo_state.lat  = c.latitude;
        Module.WebGLGeo_state.lon  = c.longitude;
        Module.WebGLGeo_state.acc  = c.accuracy;
        Module.WebGLGeo_state.time = (typeof pos.timestamp === 'number') ? pos.timestamp/1000.0 : (Date.now()/1000.0);
        Module.WebGLGeo_state.hasFix = 1;
      }, function (err) {
        // keep hasFix=0 so C# knows it's unavailable
        Module.WebGLGeo_state.hasFix = 0;
      }, opts);
    } catch (e) {
      Module.WebGLGeo_state.hasFix = 0;
    }
  },

  WebGLGeo_Stop: function () {
    if (Module.WebGLGeo_state && Module.WebGLGeo_state.watchId !== -1 && navigator.geolocation) {
      navigator.geolocation.clearWatch(Module.WebGLGeo_state.watchId);
      Module.WebGLGeo_state.watchId = -1;
    }
  },

  WebGLGeo_GetHasFix: function () {
    return (Module.WebGLGeo_state && Module.WebGLGeo_state.hasFix) ? 1 : 0;
  },
  WebGLGeo_GetLat: function ()  { return Module.WebGLGeo_state ? Module.WebGLGeo_state.lat  : 0.0; },
  WebGLGeo_GetLon: function ()  { return Module.WebGLGeo_state ? Module.WebGLGeo_state.lon  : 0.0; },
  WebGLGeo_GetAcc: function ()  { return Module.WebGLGeo_state ? Module.WebGLGeo_state.acc  : 0.0; },
  WebGLGeo_GetTime: function () { return Module.WebGLGeo_state ? Module.WebGLGeo_state.time : 0.0; }
});
