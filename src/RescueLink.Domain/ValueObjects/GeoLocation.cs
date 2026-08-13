namespace RescueLink.Domain.ValueObjects;

public sealed record GeoLocation
{
    public double Latitude { get; }
    public double Longitude { get; }

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(
        double latitude,
        double longitude)
    {
        if (double.IsNaN(latitude) ||
            double.IsInfinity(latitude) ||
            latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                "Latitude must be between -90 and 90.");
        }

        if (double.IsNaN(longitude) ||
            double.IsInfinity(longitude) ||
            longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                "Longitude must be between -180 and 180.");
        }

        return new GeoLocation(latitude, longitude);
    }
}