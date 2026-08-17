namespace RescueLink.Application.Abstractions.Authentication;

public interface IRefreshTokenGenerator
{
    GeneratedRefreshToken Generate();

    string ComputeHash(string token);
}