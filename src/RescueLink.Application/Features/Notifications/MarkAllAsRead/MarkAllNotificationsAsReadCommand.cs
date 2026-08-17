using RescueLink.Application.Abstractions.Messaging;
using RescueLink.Application.Common.Results;

namespace RescueLink.Application.Features.Notifications
    .MarkAllAsRead;

public sealed record MarkAllNotificationsAsReadCommand
    : ICommand<Result<int>>;