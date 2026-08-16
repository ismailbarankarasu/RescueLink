using RescueLink.Domain.Common;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;

namespace RescueLink.Domain.Entities;

public sealed class PetReportMatch : BaseEntity
{
    public Guid LostReportId { get; private set; }
    public Guid FoundReportId { get; private set; }
    public int Score { get; private set; }
    public double DistanceMeters { get; private set; }
    public MatchStatus Status { get; private set; }

    public bool LostOwnerConfirmed { get; private set; }
    public bool FoundOwnerConfirmed { get; private set; }

    private PetReportMatch()
    {
    }

    public static PetReportMatch Create(
        Guid lostReportId,
        Guid foundReportId,
        int score,
        double distanceMeters)
    {
        if (lostReportId == Guid.Empty)
        {
            throw new ArgumentException(
                "Lost report ID cannot be empty.",
                nameof(lostReportId));
        }

        if (foundReportId == Guid.Empty)
        {
            throw new ArgumentException(
                "Found report ID cannot be empty.",
                nameof(foundReportId));
        }

        if (lostReportId == foundReportId)
        {
            throw new ArgumentException(
                "A pet report cannot be matched with itself.");
        }

        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                "Match score must be between 0 and 100.");
        }

        if (distanceMeters < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distanceMeters),
                "Distance cannot be negative.");
        }

        var match = new PetReportMatch
        {
            LostReportId = lostReportId,
            FoundReportId = foundReportId,
            Score = score,
            DistanceMeters = distanceMeters,
            Status = MatchStatus.Suggested,
            LostOwnerConfirmed = false,
            FoundOwnerConfirmed = false
        };

        match.RaiseDomainEvent(
            new PetReportMatchSuggestedDomainEvent(
                MatchId: match.Id,
                LostReportId: match.LostReportId,
                FoundReportId: match.FoundReportId));

        return match;
    }

    public void Confirm(Guid petReportId)
    {
        EnsureReportBelongsToMatch(petReportId);

        if (Status != MatchStatus.Suggested)
        {
            throw new InvalidOperationException(
                "Only suggested matches can be confirmed.");
        }

        var confirmationChanged = false;

        if (petReportId == LostReportId &&
            !LostOwnerConfirmed)
        {
            LostOwnerConfirmed = true;
            confirmationChanged = true;
        }

        if (petReportId == FoundReportId &&
            !FoundOwnerConfirmed)
        {
            FoundOwnerConfirmed = true;
            confirmationChanged = true;
        }

        if (!confirmationChanged)
        {
            return;
        }

        if (LostOwnerConfirmed && FoundOwnerConfirmed)
        {
            Status = MatchStatus.Confirmed;

            RaiseDomainEvent(
                new PetReportMatchConfirmedDomainEvent(
                    MatchId: Id,
                    LostReportId: LostReportId,
                    FoundReportId: FoundReportId));
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(Guid petReportId)
    {
        EnsureReportBelongsToMatch(petReportId);

        if (Status != MatchStatus.Suggested)
        {
            throw new InvalidOperationException(
                "Only suggested matches can be rejected.");
        }

        Status = MatchStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureReportBelongsToMatch(Guid petReportId)
    {
        if (petReportId == Guid.Empty)
        {
            throw new ArgumentException(
                "Pet report ID cannot be empty.",
                nameof(petReportId));
        }

        if (petReportId != LostReportId &&
            petReportId != FoundReportId)
        {
            throw new InvalidOperationException(
                "Pet report does not belong to this match.");
        }
    }
}