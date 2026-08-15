using FluentAssertions;
using RescueLink.Domain.Entities;
using RescueLink.Domain.Enums;

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
    public void Confirm_ShouldChangeStatusToConfirmed()
    {
        var match = CreateSuggestedMatch();

        match.Confirm();

        match.Status.Should().Be(MatchStatus.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenMatchIsNotSuggested()
    {
        var match = CreateSuggestedMatch();
        match.Reject();

        var action = match.Confirm;

        action.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_ShouldChangeStatusToRejected()
    {
        var match = CreateSuggestedMatch();

        match.Reject();

        match.Status.Should().Be(MatchStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldThrow_WhenMatchIsNotSuggested()
    {
        var match = CreateSuggestedMatch();
        match.Confirm();

        var action = match.Reject;

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
}