using System.Net;
using System.Text.RegularExpressions;

namespace WebApp.Services;

public static class HtmlToPlainText
{
    private static readonly Regex HiddenContent = new(
        @"<(script|style)\b[^>]*>.*?</\1\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled
    );

    private static readonly Regex Comments = new(
        @"<!--.*?-->",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    private static readonly Regex LineBreaks = new(
        @"<(?:br\s*/?|/(?:p|div|li|h[1-6]|blockquote)|li\b[^>]*)>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private static readonly Regex Tags = new(
        @"<[^>]*>",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    private static readonly Regex HorizontalWhitespace = new(@"[^\S\r\n]+", RegexOptions.Compiled);

    private static readonly Regex SpaceAroundLineBreak = new(
        @"[ \t]*\r?\n[ \t]*",
        RegexOptions.Compiled
    );

    private static readonly Regex RepeatedLineBreaks = new(@"(?:\r?\n){2,}", RegexOptions.Compiled);

    public static string? Convert(string? html)
    {
        if (String.IsNullOrWhiteSpace(html))
            return html;

        var text = HiddenContent.Replace(html, String.Empty);
        text = Comments.Replace(text, String.Empty);
        text = LineBreaks.Replace(text, "\n");
        text = Tags.Replace(text, String.Empty);
        text = WebUtility.HtmlDecode(text).Replace('\u00a0', ' ');
        text = HorizontalWhitespace.Replace(text, " ");
        text = SpaceAroundLineBreak.Replace(text, "\n");
        text = RepeatedLineBreaks.Replace(text, "\n");

        return text.Trim();
    }
}
