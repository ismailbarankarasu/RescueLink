namespace RescueLink.Application.Abstractions.Authentication;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}