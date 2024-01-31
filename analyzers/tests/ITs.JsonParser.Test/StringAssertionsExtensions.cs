using FluentAssertions.Primitives;

namespace ITs.JsonParser.Test;

public static class StringAssertionsExtensions
{
    private const string WindowsLineEnding = "\r\n";
    private const string UnixLineEnding = "\n";

    public static void BeIgnoringLineEndings(this StringAssertions stringAssertions, string expected) =>
        stringAssertions.Subject.ToUnixLineEndings().Should().Be(expected.ToUnixLineEndings());

    private static string ToUnixLineEndings(this string value) =>
        value.Replace(WindowsLineEnding, UnixLineEnding);
}
