using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application
    .Features.Users.UpdateCurrent;

public sealed class UpdateCurrentUserProfileCommandHandler
    : IRequestHandler<
        UpdateCurrentUserProfileCommand,
        Result>
{
    private readonly ICurrentUserService
        _currentUserService;

    private readonly IIdentityService
        _identityService;

    public UpdateCurrentUserProfileCommandHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<Result> Handle(
        UpdateCurrentUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure(
                UserProfileErrors.Unauthenticated);
        }

        var profile = new UpdateUserProfileInfo(
            FirstName: request.FirstName,
            LastName: request.LastName,
            PhoneNumber: request.PhoneNumber,
            CountryCode: request.CountryCode,
            City: request.City,
            PreferredLanguage:
                request.PreferredLanguage,
            TimeZoneId: request.TimeZoneId);

        return await _identityService
            .UpdateUserProfileAsync(
                _currentUserService.UserId.Value,
                profile,
                cancellationToken);
    }
}