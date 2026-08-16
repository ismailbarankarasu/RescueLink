using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;
using RescueLink.Domain.Events;

namespace RescueLink.Domain.Tests.Entities;

public sealed class PetReportMatchTests
{
    [Fact]
    public void Create_ShouldCreateSuggestedMatch_WhenValuesAreValid()
    {
        var lostReportId = Guid.NewGuid();
        var foundReportId = Guid.NewGuid();

        var match = PetReportMatch.Create(
            lostReportId: lostReportId,
            foundReportId: foundReportId,
            score: 85,
            distanceMeters: 1250);

        match.LostReportId.Should().Be(lostReportId);
        match.FoundReportId.Should().Be(foundReportId);
        match.Score.Should().Be(85);
        match.DistanceMeters.Should().Be(1250);
        match.Status.Should().Be(MatchStatus.Suggested);

        match.LostOwnerConfirmed.Should().BeFalse();
        match.FoundOwnerConfirmed.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldThrow_WhenLostReportIdIsEmpty()
    {
        var action = () => PetReportMatch.Create(
            lostReportId: Guid.Empty,
            foundReportId: Guid.NewGuid(),
            score: 80,
            distanceMeters: 1000);

        action.Should()
            .Throw<ArgumentException>()
            .WithParameterName("lostReportId");
    }

    [Fact]
    public void Create_ShouldThrow_WhenFoundReportIdIsEmpty()
    {
        var action = () => PetReportMatch.Create(
            lostReportId: Guid.NewGuid(),
            foundReportId: Guid.Empty,
            score: 80,
            distanceMeters: 1000);

        action.Should()
            .Throw<ArgumentException>()
            .WithParameterName("foundReportId");
    }

    [Fact]
    public void Create_ShouldThrow_WhenReportIsMatchedWithItself()
    {
        var reportId = Guid.NewGuid();

        var action = () => PetReportMatch.Create(
            lostReportId: reportId,
            foundReportId: reportId,
            score: 80,
            distanceMeters: 1000);

        action.Should()
            .Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_ShouldThrow_WhenScoreIsInvalid(int score)
    {
        var action = () => PetReportMatch.Create(
            lostReportId: Guid.NewGuid(),
            foundReportId: Guid.NewGuid(),
            score: score,
            distanceMeters: 1000);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("score");
    }

    [Fact]
    public void Create_ShouldThrow_WhenDistanceIsNegative()
    {
        var action = () => PetReportMatch.Create(
            lostReportId: Guid.NewGuid(),
            foundReportId: Guid.NewGuid(),
            score: 80,
            distanceMeters: -1);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("distanceMeters");
    }

    [Fact]
    public void Confirm_ShouldConfirmLostOwnerOnly()
    {
        var match = CreateSuggestedMatch();

        match.Confirm(match.LostReportId);

        match.LostOwnerConfirmed.Should().BeTrue();
        match.FoundOwnerConfirmed.Should().BeFalse();

        match.Status.Should().Be(MatchStatus.Suggested);
    }

    [Fact]
    public void Confirm_ShouldConfirmFoundOwnerOnly()
    {
        var match = CreateSuggestedMatch();

        match.Confirm(match.FoundReportId);

        match.LostOwnerConfirmed.Should().BeFalse();
        match.FoundOwnerConfirmed.Should().BeTrue();

        match.Status.Should().Be(MatchStatus.Suggested);
    }

    [Fact]
    public void Confirm_ShouldChangeStatusToConfirmed_WhenBothOwnersConfirm()
    {
        var match = CreateSuggestedMatch();

        match.Confirm(match.LostReportId);
        match.Confirm(match.FoundReportId);

        match.LostOwnerConfirmed.Should().BeTrue();
        match.FoundOwnerConfirmed.Should().BeTrue();

        match.Status.Should().Be(MatchStatus.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldBeIdempotent_WhenSameOwnerConfirmsTwice()
    {
        var match = CreateSuggestedMatch();

        match.Confirm(match.LostReportId);
        match.Confirm(match.LostReportId);

        match.LostOwnerConfirmed.Should().BeTrue();
        match.FoundOwnerConfirmed.Should().BeFalse();

        match.Status.Should().Be(MatchStatus.Suggested);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenReportDoesNotBelongToMatch()
    {
        var match = CreateSuggestedMatch();

        var action = () => match.Confirm(Guid.NewGuid());

        action.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenMatchIsRejected()
    {
        var match = CreateSuggestedMatch();

        match.Reject(match.LostReportId);

        var action = () =>
            match.Confirm(match.FoundReportId);

        action.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_ShouldChangeStatusToRejected()
    {
        var match = CreateSuggestedMatch();

        match.Reject(match.LostReportId);

        match.Status.Should().Be(MatchStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldWork_WhenOneOwnerPreviouslyConfirmed()
    {
        var match = CreateSuggestedMatch();

        match.Confirm(match.LostReportId);
        match.Reject(match.FoundReportId);

        match.LostOwnerConfirmed.Should().BeTrue();
        match.FoundOwnerConfirmed.Should().BeFalse();
        match.Status.Should().Be(MatchStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldThrow_WhenReportDoesNotBelongToMatch()
    {
        var match = CreateSuggestedMatch();

        var action = () => match.Reject(Guid.NewGuid());

        action.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_ShouldThrow_WhenMatchIsConfirmed()
    {
        var match = CreateSuggestedMatch();

        match.Confirm(match.LostReportId);
        match.Confirm(match.FoundReportId);

        var action = () =>
            match.Reject(match.LostReportId);

        action.Should()
            .Throw<InvalidOperationException>();
    }

    private static PetReportMatch CreateSuggestedMatch()
    {
        return PetReportMatch.Create(
            lostReportId: Guid.NewGuid(),
            foundReportId: Guid.NewGuid(),
            score: 80,
            distanceMeters: 1000);
    }
    [Fact]
    public void Confirm_ShouldRaiseDomainEvent_WhenBothOwnersConfirm()
    {
        var match = CreateSuggestedMatch();

        match.Confirm(match.LostReportId);

        match.DomainEvents.Should().BeEmpty();

        match.Confirm(match.FoundReportId);

        var domainEvent = match.DomainEvents
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<PetReportMatchConfirmedDomainEvent>()
            .Subject;

        domainEvent.MatchId.Should().Be(match.Id);
        domainEvent.LostReportId.Should().Be(match.LostReportId);
        domainEvent.FoundReportId.Should().Be(match.FoundReportId);
    }
}