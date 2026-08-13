using MediatR;

namespace RescueLink.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}