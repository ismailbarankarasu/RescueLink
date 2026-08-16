using Dapper;
using RescueLink.Application.Abstractions.Data;
using RescueLink.Application.Common.Pagination;
using RescueLink.Application.Features.Notifications.GetList;
using RescueLink.Domain.Enums;

namespace RescueLink.Persistence.Queries;

internal sealed class NotificationReadService(
    IDbConnectionFactory connectionFactory)
    : INotificationReadService
{
    public async Task<
        PagedResult<NotificationListItemResponse>> GetAsync(
            Guid userId,
            int page,
            int pageSize,
            bool unreadOnly,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT_BIG(1)
            FROM dbo.UserNotifications AS notification
            WHERE notification.UserId = @UserId
              AND (
                    @UnreadOnly = 0
                    OR notification.IsRead = 0
                  );

            SELECT
                notification.Id,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.RelatedEntityId,
                notification.IsRead,
                notification.ReadAt,
                notification.CreatedAt
            FROM dbo.UserNotifications AS notification
            WHERE notification.UserId = @UserId
              AND (
                    @UnreadOnly = 0
                    OR notification.IsRead = 0
                  )
            ORDER BY notification.CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = new
        {
            UserId = userId,
            UnreadOnly = unreadOnly,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        await using var connection =
            connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        using var result =
            await connection.QueryMultipleAsync(command);

        var totalCountLong =
            await result.ReadSingleAsync<long>();

        var rows =
            (await result.ReadAsync<NotificationListItemRow>())
            .ToArray();

        var items = rows
            .Select(row =>
                new NotificationListItemResponse(
                    Id: row.Id,
                    Type: row.Type,
                    Title: row.Title,
                    Message: row.Message,
                    RelatedEntityId:
                        row.RelatedEntityId,
                    IsRead: row.IsRead,
                    ReadAt: row.ReadAt,
                    CreatedAt: row.CreatedAt))
            .ToArray();

        return new PagedResult<NotificationListItemResponse>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: checked((int)totalCountLong));
    }

    private sealed class NotificationListItemRow
    {
        public Guid Id { get; init; }
        public NotificationType Type { get; init; }

        public string Title { get; init; } =
            string.Empty;

        public string Message { get; init; } =
            string.Empty;

        public Guid? RelatedEntityId { get; init; }
        public bool IsRead { get; init; }
        public DateTimeOffset? ReadAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}