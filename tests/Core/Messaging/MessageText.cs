namespace BCIT_Social.Core.Messaging;

public enum MessageTextError
{
    None,
    Empty,
    TooLong,
}

public readonly record struct MessageTextResult(string? Text, MessageTextError Error)
{
    public bool IsValid => Error == MessageTextError.None;
}

/// <summary>Rules for a direct message body (extracted from MessagesController.Send).</summary>
public static class MessageText
{
    public const int MAX_LENGTH = 5000;

    public static MessageTextResult Normalize(string? raw)
    {
        var body = raw?.Trim();
        if (string.IsNullOrEmpty(body)) return new(null, MessageTextError.Empty);
        if (body.Length > MAX_LENGTH) return new(null, MessageTextError.TooLong);
        return new(body, MessageTextError.None);
    }
}
