using FluentAssertions;
using RescueLink.Application.Features.PetReports.Photos.Upload;

namespace RescueLink.Application.Tests.Features.PetReports.Photos.Upload;

public sealed class UploadPetReportPhotoCommandValidatorTests
{
    private readonly UploadPetReportPhotoCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldSucceed_WhenFileIsValid()
    {
        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateValidCommand(content);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("image/gif")]
    public async Task Validate_ShouldFail_WhenContentTypeIsInvalid(
        string contentType)
    {
        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateValidCommand(content) with
        {
            ContentType = contentType
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            error => error.PropertyName ==
                     nameof(command.ContentType));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5 * 1024 * 1024 + 1)]
    public async Task Validate_ShouldFail_WhenLengthIsInvalid(
        long length)
    {
        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateValidCommand(content) with
        {
            Length = length
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            error => error.PropertyName ==
                     nameof(command.Length));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPetReportIdIsEmpty()
    {
        using var content = new MemoryStream([1, 2, 3]);

        var command = CreateValidCommand(content) with
        {
            PetReportId = Guid.Empty
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(
            error => error.PropertyName ==
                     nameof(command.PetReportId));
    }

    private static UploadPetReportPhotoCommand CreateValidCommand(
        Stream content)
    {
        return new UploadPetReportPhotoCommand(
            PetReportId: Guid.NewGuid(),
            Content: content,
            FileName: "pet-photo.jpg",
            ContentType: "image/jpeg",
            Length: content.Length);
    }
}