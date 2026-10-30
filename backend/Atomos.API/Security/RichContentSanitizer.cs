using AngleSharp.Dom;
using Ganss.Xss;

namespace Atomos.API.Security;

public static class RichContentSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    public static string Sanitize(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;
        // Legacy seed content may contain comparison signs but no markup.
        if (!LooksLikeMarkup(content)) return content.Trim();
        return Sanitizer.Sanitize(content).Trim();
    }

    private static bool LooksLikeMarkup(string content)
    {
        for (var index = content.IndexOf('<'); index >= 0; index = content.IndexOf('<', index + 1))
        {
            if (index + 1 >= content.Length) return true;
            var next = content[index + 1];
            if (!char.IsWhiteSpace(next) && !char.IsDigit(next)) return true;
        }
        return false;
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith([
            "p", "div", "span", "br", "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li", "b", "strong", "i", "em", "u", "sub", "sup",
            "blockquote", "hr", "a", "img", "figure", "figcaption"
        ]);
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith([
            "href", "src", "alt", "title", "style", "class", "target", "rel"
        ]);
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedCssProperties.UnionWith([
            "text-align", "font-weight", "font-style", "text-decoration", "vertical-align"
        ]);
        sanitizer.AllowedAtRules.Clear();
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);
        sanitizer.FilterUrl += (_, args) =>
        {
            if (args.Tag.LocalName != "a" &&
                args.SanitizedUrl?.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) == true)
                args.SanitizedUrl = null;
        };
        sanitizer.PostProcessNode += (_, args) =>
        {
            if (args.Node is not IElement element) return;
            // Only the editor's image-block class is part of the content contract.
            if (element.HasAttribute("class"))
            {
                if (element.ClassList.Contains("admin-image-block") && element.LocalName == "p")
                    element.SetAttribute("class", "admin-image-block");
                else
                    element.RemoveAttribute("class");
            }
            if (element.GetAttribute("target") == "_blank" && element.LocalName == "a")
                element.SetAttribute("rel", "noopener noreferrer");
            else
                element.RemoveAttribute("target");
        };
        return sanitizer;
    }
}
