namespace Atomos.Application.Common;

public sealed class EmailMessage
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string PlainTextBody { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
}
