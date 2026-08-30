namespace RescueLink.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public DateTimeOffset OccurredOnUtc { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset NextAttemptOnUtc { get; private set; }
    private OutboxMessage()
    {
    }

    public static OutboxMessage Create(
        DateTimeOffset occurredOnUtc,
        string type,
        string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = occurredOnUtc,
            Type = type,
            Content = content,
            RetryCount = 0,
            NextAttemptOnUtc = occurredOnUtc
        };
    }

    public void MarkAsProcessed(
        DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void MarkAsFailed(
        string error,
        DateTimeOffset nextAttemptOnUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        Error = error.Length <= 2000
            ? error
            : error[..2000];

        RetryCount++;
        NextAttemptOnUtc = nextAttemptOnUtc;
    }
}