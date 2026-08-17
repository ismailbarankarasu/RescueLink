using MediatR;
using RescueLink.Application.Abstractions.Authentication;
using RescueLink.Application.Abstractions.Persistence;
using RescueLink.Application.Common.Results;
using RescueLink.Domain.Enums;

namespace RescueLink.Application.Features
    .PetReportMatches.GetContact;

public sealed class GetMatchContactQueryHandler
    : IRequestHandler<
        GetMatchContactQuery,
        Result<CounterpartContactResponse>>
{
    private readonly IPetReportMatchRepository
        _matchRepository;

    private readonly IPetReportRepository
        _petReportRepository;

    private readonly IIdentityService
        _identityService;

    private readonly ICurrentUserService
        _currentUserService;

    public GetMatchContactQueryHandler(
        IPetReportMatchRepository matchRepository,
        IPetReportRepository petReportRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _matchRepository = matchRepository;
        _petReportRepository = petReportRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<
        Result<CounterpartContactResponse>> Handle(
            GetMatchContactQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure<
                CounterpartContactResponse>(
                    PetReportMatchErrors.Unauthenticated);
        }

        var match =
            await _matchRepository.GetByIdReadOnlyAsync(
                request.MatchId,
                cancellationToken);

        if (match is null)
        {
            return Result.Failure<
                CounterpartContactResponse>(
                    PetReportMatchErrors.NotFound(
                        request.MatchId));
        }

        var reportIds = new[]
        {
            match.LostReportId,
            match.FoundReportId
        };

        var reports =
            await _petReportRepository
                .GetByIdsReadOnlyAsync(
                    reportIds,
                    cancellationToken);

        var lostReport = reports.SingleOrDefault(
            report =>
                report.Id == match.LostReportId);

        var foundReport = reports.SingleOrDefault(
            report =>
                report.Id == match.FoundReportId);

        if (lostReport is null ||
            foundReport is null)
        {
            return Result.Failure<
                CounterpartContactResponse>(
                    PetReportMatchErrors
                        .RelatedReportsNotFound);
        }

        var currentUserId =
            _currentUserService.UserId.Value;

        Guid counterpartUserId;

        if (lostReport.UserId == currentUserId)
        {
            counterpartUserId = foundReport.UserId;
        }
        else if (foundReport.UserId == currentUserId)
        {
            counterpartUserId = lostReport.UserId;
        }
        else
        {
            return Result.Failure<
                CounterpartContactResponse>(
                    PetReportMatchErrors.Forbidden);
        }

        if (match.Status != MatchStatus.Confirmed)
        {
            return Result.Failure<
                CounterpartContactResponse>(
                    PetReportMatchErrors
                        .ContactNotAvailable);
        }

        var contact =
            await _identityService.GetUserContactAsync(
                counterpartUserId,
                cancellationToken);

        if (contact is null)
        {
            return Result.Failure<
                CounterpartContactResponse>(
                    PetReportMatchErrors
                        .ContactNotAvailable);
        }

        var response =
            new CounterpartContactResponse(
                UserId: contact.UserId,
                FirstName: contact.FirstName,
                LastName: contact.LastName,
                Email: contact.Email);

        return Result.Success(response);
    }
}