using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Cancel;

public sealed record CancelPetReportCommand(Guid PetReportId)
    : ICommand<Result>;