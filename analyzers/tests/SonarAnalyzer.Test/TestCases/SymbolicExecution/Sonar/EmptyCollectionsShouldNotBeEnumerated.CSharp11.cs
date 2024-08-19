using System;

public class Sample
{
    void Spans(Span<char> chars, ReadOnlySpan<char> readonlyChars)
    {
        if (chars is "")
        {
            chars.Clear(); // FN
        }

        if (readonlyChars is "")
        {
            var nothing1 = readonlyChars[5]; // FN
        }

        if (chars.IsEmpty)
        {
            chars.Clear(); // FN
        }

        if (readonlyChars.IsEmpty)
        {
            var nothing2 = readonlyChars[5]; // FN
        }
    }

    void ListPattern(byte[] bytes)
    {
        if (bytes is [])
        {
            bytes.Clone(); // FN
        }
    }
}
