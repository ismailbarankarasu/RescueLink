using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RescueLink.Domain.ValueObjects;

namespace RescueLink.Persistence.Converters;

public sealed class GeoLocationConverter
    : ValueConverter<GeoLocation, Point>
{
    public GeoLocationConverter()
        : base(
            location => ToPoint(location),
            point => ToGeoLocation(point))
    {
    }

    private static Point ToPoint(GeoLocation location)
    {
        return new Point(
            x: location.Longitude,
            y: location.Latitude)
        {
            SRID = 4326
        };
    }

    private static GeoLocation ToGeoLocation(Point point)
    {
        return GeoLocation.Create(
            latitude: point.Y,
            longitude: point.X);
    }
}