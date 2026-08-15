using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.Resolve;

public sealed record ResolvePetReportCommand(Guid PetReportId)
    : ICommand<Result>;