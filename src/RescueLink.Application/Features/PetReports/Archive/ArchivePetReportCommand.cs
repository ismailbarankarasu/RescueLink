using MediatR;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features
    .PetReports.Archive;

public sealed record ArchivePetReportCommand(
    Guid PetReportId)
    : IRequest<Result>;