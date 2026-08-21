using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Users.GetCurrent;

public sealed class GetCurrentUserProfileQueryHandler
    : IRequestHandler<
        GetCurrentUserProfileQuery,
        Result<GetCurrentUserProfileResponse>>
{
    private readonly ICurrentUserService
        _currentUserService;

    private readonly IIdentityService
        _identityService;

    public GetCurrentUserProfileQueryHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<
        Result<GetCurrentUserProfileResponse>> Handle(
            GetCurrentUserProfileQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<
                GetCurrentUserProfileResponse>(
                    UserProfileErrors.Unauthenticated);
        }

        var profile =
            await _identityService.GetUserProfileAsync(
                _currentUserService.UserId.Value,
                cancellationToken);

        if (profile is null)
        {
            return Result.Failure<
                GetCurrentUserProfileResponse>(
                    UserProfileErrors.NotFound);
        }

        var response =
            new GetCurrentUserProfileResponse(
                UserId: profile.UserId,
                FirstName: profile.FirstName,
                LastName: profile.LastName,
                Email: profile.Email,
                PhoneNumber: profile.PhoneNumber,
                CountryCode: profile.CountryCode,
                City: profile.City,
                PreferredLanguage:
                    profile.PreferredLanguage,
                TimeZoneId: profile.TimeZoneId,
                CreatedAt: profile.CreatedAt);

        return Result.Success(response);
    }
}