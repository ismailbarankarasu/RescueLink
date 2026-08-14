using RescueLink.Application.Common.Results;
using RescueLink.Domain.Entities;

namespace RescueLink.Application.Features.PetReports;

public static class PetReportErrors
{
    public static readonly Error Unauthenticated = new(
        "Authentication.Unauthenticated",
        "The current user is not authenticated.");

    public static Error NotFound(Guid reportId)
    {
        return new Error(
            "PetReport.NotFound",
            $"Pet report '{reportId}' was not found.");
    }

    public static readonly Error Forbidden = new(
         "PetReport.Forbidden",
         "You are not allowed to modify this pet report.");

    public static readonly Error MaximumPhotoCountReached = new(
         "PetReport.MaximumPhotoCountReached",
         $"A pet report can contain at most " +
         $"{PetReport.MaximumPhotoCount} photos.");
}