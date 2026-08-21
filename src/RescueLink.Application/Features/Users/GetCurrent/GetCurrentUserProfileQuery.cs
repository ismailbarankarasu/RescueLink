using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Users.GetCurrent;

public sealed record GetCurrentUserProfileQuery
    : IQuery<Result<GetCurrentUserProfileResponse>>;