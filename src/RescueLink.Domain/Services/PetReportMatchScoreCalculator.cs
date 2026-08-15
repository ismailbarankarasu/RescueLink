using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;

namespace RescueLink.Domain.Services;

public static class PetReportMatchScoreCalculator
{
    public const double MaximumDistanceMeters = 10_000;
    public const int MinimumSuggestedScore = 50;

    public static int Calculate(
        PetReport firstReport,
        PetReport secondReport,
        double distanceMeters)
    {
        ArgumentNullException.ThrowIfNull(firstReport);
        ArgumentNullException.ThrowIfNull(secondReport);

        if (distanceMeters < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distanceMeters),
                "Distance cannot be negative.");
        }

        if (firstReport.Id == secondReport.Id)
        {
            return 0;
        }

        if (firstReport.Status != ReportStatus.Active ||
            secondReport.Status != ReportStatus.Active)
        {
            return 0;
        }

        if (firstReport.ReportType == secondReport.ReportType)
        {
            return 0;
        }

        if (firstReport.Species != secondReport.Species)
        {
            return 0;
        }

        if (distanceMeters > MaximumDistanceMeters)
        {
            return 0;
        }

        var score = 30;

        if (BreedsMatch(firstReport.Breed, secondReport.Breed))
        {
            score += 20;
        }

        score += CalculateColorScore(
            firstReport,
            secondReport);

        if (GendersMatch(
                firstReport.Gender,
                secondReport.Gender))
        {
            score += 10;
        }

        score += CalculateDistanceScore(distanceMeters);

        return Math.Min(score, 100);
    }

    private static bool BreedsMatch(
        string? firstBreed,
        string? secondBreed)
    {
        if (string.IsNullOrWhiteSpace(firstBreed) ||
            string.IsNullOrWhiteSpace(secondBreed))
        {
            return false;
        }

        return string.Equals(
            firstBreed.Trim(),
            secondBreed.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool GendersMatch(
        AnimalGender firstGender,
        AnimalGender secondGender)
    {
        return firstGender != AnimalGender.Unknown &&
               secondGender != AnimalGender.Unknown &&
               firstGender == secondGender;
    }

    private static int CalculateColorScore(
        PetReport firstReport,
        PetReport secondReport)
    {
        if (firstReport.PrimaryColor ==
            secondReport.PrimaryColor)
        {
            return 20;
        }

        var firstColors = GetColors(firstReport);
        var secondColors = GetColors(secondReport);

        return firstColors.Overlaps(secondColors)
            ? 10
            : 0;
    }

    private static HashSet<AnimalColor> GetColors(
        PetReport report)
    {
        var colors = new HashSet<AnimalColor>
        {
            report.PrimaryColor
        };

        if (report.SecondaryColor.HasValue)
        {
            colors.Add(report.SecondaryColor.Value);
        }

        return colors;
    }

    private static int CalculateDistanceScore(
        double distanceMeters)
    {
        return distanceMeters switch
        {
            <= 1_000 => 20,
            <= 3_000 => 15,
            <= 5_000 => 10,
            <= 10_000 => 5,
            _ => 0
        };
    }
}