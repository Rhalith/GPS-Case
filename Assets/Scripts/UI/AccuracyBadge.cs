using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>Simple color badge that reflects GPS accuracy quality.</summary>
    public sealed class AccuracyBadge : MonoBehaviour
    {
        [SerializeField] private Image target;
        [SerializeField, Min(0f)] private float goodThresholdMeters = 5f;
        [SerializeField, Min(0f)] private float okThresholdMeters   = 15f;

        [Header("Colors")]
        [SerializeField] private Color goodColor   = new (0.20f, 0.75f, 0.36f); // green
        [SerializeField] private Color okColor     = new (0.98f, 0.77f, 0.18f); // yellow
        [SerializeField] private Color poorColor   = new (0.85f, 0.20f, 0.20f); // red
        [SerializeField] private Color unknownColor= new (0.50f, 0.50f, 0.50f); // gray

        private enum Quality { Unknown, Good, Ok, Poor }

        private void Awake()
        {
            if (!target) target = GetComponent<Image>();
        }

        public void SetAccuracy(float meters)
        {
            if (!target) return;

            Quality q = Quality.Unknown;
            if (meters > 0f)
            {
                if (meters <= goodThresholdMeters)      q = Quality.Good;
                else if (meters <= okThresholdMeters)   q = Quality.Ok;
                else                                     q = Quality.Poor;
            }

            target.color = q switch
            {
                Quality.Good    => goodColor,
                Quality.Ok      => okColor,
                Quality.Poor    => poorColor,
                _               => unknownColor
            };
        }

        public void SetUnknown() => SetAccuracy(-1f);
    }
}