using MediatR;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features
    .PetReports.Restore;

public sealed record RestorePetReportCommand(
    Guid PetReportId)
    : IRequest<Result>;