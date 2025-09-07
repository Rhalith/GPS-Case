using UnityEngine;
using Location;
using TMPro;

namespace UI
{
    /// <summary>Simple HUD: shows latest GPS values; optional wire from GpsController via public method.</summary>
    public sealed class GpsHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text latitudeText;
        [SerializeField] private TMP_Text longitudeText;
        [SerializeField] private TMP_Text accuracyText;
        [SerializeField] private TMP_Text statusText;

        public void SetStatus(string status)
        {
            if (statusText) statusText.text = status;
        }

        public void SetSample(LocationData data)
        {
            if (latitudeText)  latitudeText.text  = $"Lat: {data.Latitude:F6}";
            if (longitudeText) longitudeText.text = $"Lon: {data.Longitude:F6}";
            if (accuracyText)  accuracyText.text  = $"Acc: {data.AccuracyMeters:F1} m";
        }
    }
}