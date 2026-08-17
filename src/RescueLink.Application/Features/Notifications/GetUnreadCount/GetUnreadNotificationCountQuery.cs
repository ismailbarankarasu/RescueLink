using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications
    .GetUnreadCount;

public sealed record GetUnreadNotificationCountQuery
    : IQuery<Result<int>>;