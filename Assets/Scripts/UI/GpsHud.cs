using UnityEngine;
using TMPro;
using Location;

namespace UI
{
    public sealed class GpsHud : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text currentLatText;
        [SerializeField] private TMP_Text currentLonText;
        [SerializeField] private TMP_Text lastLatText;
        [SerializeField] private TMP_Text lastLonText;
        [SerializeField] private TMP_Text statusText;

        [Header("Accuracy")]
        [SerializeField] private TMP_Text accuracyText;
        [SerializeField] private AccuracyBadge accuracyBadge;

        [Header("Debug (optional)")]
        [SerializeField] private TMP_Text decisionText;  // <-- add a TMP_Text in Canvas and assign here

        public void SetStatus(string status)
        {
            if (statusText) statusText.text = status;
            if (status == "Initializing…" || status == "Location unavailable" || status == "Stopped")
                accuracyBadge?.SetUnknown();
        }

        public void SetCurrent(LocationData data)
        {
            if (currentLatText) currentLatText.text = $"Current Lat: {data.Latitude:F6}";
            if (currentLonText) currentLonText.text = $"Current Lon: {data.Longitude:F6}";
        }

        public void SetLastUpdated(LocationData data)
        {
            if (lastLatText) lastLatText.text = $"Last Lat: {data.Latitude:F6}";
            if (lastLonText)  lastLonText.text = $"Last Lon: {data.Longitude:F6}";
            if (accuracyText) accuracyText.text = $"Acc: {data.AccuracyMeters:F1} m";
            accuracyBadge?.SetAccuracy((float)data.AccuracyMeters);
        }

        public void SetDecision(string msg)
        {
            if (decisionText) decisionText.text = msg;
        }
    }
}