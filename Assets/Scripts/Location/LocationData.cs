namespace Location
{
    /// <summary>Immutable GPS sample.</summary>
    public readonly struct LocationData
    {
        public readonly double Latitude;
        public readonly double Longitude;
        public readonly double AccuracyMeters;
        public readonly double Timestamp;

        public LocationData(double lat, double lon, double accuracyMeters, double timestamp)
        {
            Latitude = lat;
            Longitude = lon;
            AccuracyMeters = accuracyMeters;
            Timestamp = timestamp;
        }
    }
}