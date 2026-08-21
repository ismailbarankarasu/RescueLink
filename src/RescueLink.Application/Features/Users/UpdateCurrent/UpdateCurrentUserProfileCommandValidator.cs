using System.Globalization;
using FluentValidation;

namespace RescueLink.Application
    .Features.Users.UpdateCurrent;

public sealed class UpdateCurrentUserProfileCommandValidator
    : AbstractValidator<UpdateCurrentUserProfileCommand>
{
    public UpdateCurrentUserProfileCommandValidator()
    {
        RuleFor(command => command.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.PhoneNumber)
            .Matches(@"^\+[1-9]\d{7,14}$")
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.PhoneNumber))
            .WithMessage(
                "Phone number must use E.164 format, for example +905551234567.");

        RuleFor(command => command.CountryCode)
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.CountryCode))
            .WithMessage(
                "Country code must be a two-letter ISO code.");

        RuleFor(command => command.City)
            .MaximumLength(100)
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.City));

        RuleFor(command => command.PreferredLanguage)
            .NotEmpty()
            .MaximumLength(10)
            .Must(BeValidCulture)
            .WithMessage(
                "Preferred language must be a valid culture code.");

        RuleFor(command => command.TimeZoneId)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidTimeZone)
            .WithMessage(
                "Time zone ID is invalid.");
    }

    private static bool BeValidCulture(
        string cultureName)
    {
        try
        {
            _ = CultureInfo.GetCultureInfo(
                cultureName);

            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private static bool BeValidTimeZone(
        string timeZoneId)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(
                timeZoneId);

            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}