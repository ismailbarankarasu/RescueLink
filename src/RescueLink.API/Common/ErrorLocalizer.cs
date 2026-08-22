using System.Globalization;
using RescueLink.Application.Common.Results;
using RescueLink.Application.Localization;
using RescueLink.Domain.Entities;

namespace RescueLink.API.Common;

internal sealed class ErrorLocalizer
    : IErrorLocalizer
{
    public Error Localize(Error error)
    {
        var localizedMessage = error.Code switch
        {
            "Authentication.InvalidCredentials" =>
                ErrorMessages
                    .AuthenticationInvalidCredentials,

            "Authentication.EmailAlreadyInUse" =>
                ErrorMessages
                    .AuthenticationEmailAlreadyInUse,

            "Authentication.InvalidRefreshToken" =>
                ErrorMessages
                    .AuthenticationInvalidRefreshToken,

            "Authentication.Unauthenticated" =>
                ErrorMessages
                    .AuthenticationUnauthenticated,

            "PetReport.NotFound" =>
                ErrorMessages
                    .PetReportNotFound,

            "PetReport.Forbidden" =>
                ErrorMessages
                    .PetReportForbidden,

            "PetReport.MaximumPhotoCountReached" =>
                string.Format(
                    CultureInfo.CurrentUICulture,
                    ErrorMessages
                        .PetReportMaximumPhotoCountReached,
                    PetReport.MaximumPhotoCount),

            "PetReport.InvalidPhotoFile" =>
                ErrorMessages
                    .PetReportInvalidPhotoFile,

            "PetReport.PhotoNotFound" =>
                ErrorMessages
                    .PetReportPhotoNotFound,

            "PetReport.NotActive" =>
                ErrorMessages
                    .PetReportNotActive,
            "PetReportMatch.Forbidden" =>
                ErrorMessages
                    .PetReportMatchForbidden,

            "PetReportMatch.NotSuggested" =>
                ErrorMessages
                    .PetReportMatchNotSuggested,

            "PetReportMatch.ReportsNotActive" =>
                ErrorMessages
                    .PetReportMatchReportsNotActive,

            "PetReportMatch.ContactNotAvailable" =>
                ErrorMessages
                    .PetReportMatchContactNotAvailable,

            "PetReportMatch.RelatedReportsNotFound" =>
                ErrorMessages
                    .PetReportMatchRelatedReportsNotFound,

            "PetReportMatch.NotFound" =>
                ErrorMessages
                    .PetReportMatchNotFound,

            _ => error.Message
        };

        return new Error(
            error.Code,
            localizedMessage);
    }
}