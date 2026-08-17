using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features
    .PetReportMatches.GetContact;

public sealed record GetMatchContactQuery(
    Guid MatchId)
    : IQuery<Result<CounterpartContactResponse>>;