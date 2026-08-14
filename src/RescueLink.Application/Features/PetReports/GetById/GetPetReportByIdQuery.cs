using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.PetReports.GetById;

public sealed record GetPetReportByIdQuery(Guid Id)
    : IQuery<Result<PetReportDetailResponse>>;