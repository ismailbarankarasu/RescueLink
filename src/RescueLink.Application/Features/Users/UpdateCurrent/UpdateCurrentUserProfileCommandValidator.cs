using System.Globalization;
using FluentValidation;
using RescueLink.Application.Localization;

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
            .WithMessage(_ =>
                ValidationMessages.PhoneNumberInvalid);

        RuleFor(command => command.CountryCode)
            .Length(2)
            .Matches("^[A-Za-z]{2}$")
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.CountryCode))
            .WithMessage(_ =>
                ValidationMessages.CountryCodeInvalid);

        RuleFor(command => command.City)
            .MaximumLength(100)
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.City));

        RuleFor(command => command.PreferredLanguage)
            .NotEmpty()
            .MaximumLength(10)
            .Must(BeValidCulture)
            .WithMessage(_ =>
                ValidationMessages
                    .PreferredLanguageInvalid);

        RuleFor(command => command.TimeZoneId)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidTimeZone)
            .WithMessage(_ =>
                ValidationMessages.TimeZoneInvalid);
    }

    private static bool BeValidCulture(
        string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return false;
        }

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
        string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return false;
        }

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