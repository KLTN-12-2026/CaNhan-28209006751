namespace Atomos.Domain.About;

public sealed class AboutContent
{
    public int IDAboutContent { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
