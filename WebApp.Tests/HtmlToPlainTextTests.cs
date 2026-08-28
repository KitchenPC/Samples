using WebApp.Services;
using Xunit;

namespace WebApp.Tests;

public sealed class HtmlToPlainTextTests
{
    [Fact]
    public void ConvertsParagraphsAndEntitiesToReadableText()
    {
        const string html =
            "<p><b>Line one&nbsp;here.</b></p><p>Line two &amp; more.<br>Last line.</p>";

        var result = HtmlToPlainText.Convert(html);

        Assert.Equal("Line one here.\nLine two & more.\nLast line.", result);
    }

    [Fact]
    public void ConvertsListsToSeparateLines()
    {
        const string html = "<ul><li>First</li><li>Second</li></ul>";

        var result = HtmlToPlainText.Convert(html);

        Assert.Equal("First\nSecond", result);
    }

    [Fact]
    public void RemovesScriptStyleAndCommentContent()
    {
        const string html =
            "<p>Keep me.</p><script>alert('no')</script><style>p{color:red}</style><!-- hidden -->";

        var result = HtmlToPlainText.Convert(html);

        Assert.Equal("Keep me.", result);
    }

    [Fact]
    public void LeavesDecodedMarkupAsPlainTextForReactToEscape()
    {
        const string html = "Use &lt;strong&gt;care&lt;/strong&gt; when stirring.";

        var result = HtmlToPlainText.Convert(html);

        Assert.Equal("Use <strong>care</strong> when stirring.", result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    public void PreservesEmptyInput(string? input, string? expected)
    {
        Assert.Equal(expected, HtmlToPlainText.Convert(input));
    }
}
