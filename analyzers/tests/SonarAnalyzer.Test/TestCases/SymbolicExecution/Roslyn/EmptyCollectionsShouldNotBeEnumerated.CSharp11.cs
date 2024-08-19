using System;

public class Sample
{
    void Spans(Span<char> chars, ReadOnlySpan<char> readonlyChars)
    {
        if (chars is "")
        {
            chars.Clear();          // FN
        }

        if (readonlyChars is "")
        {
            _ = readonlyChars[5];   // FN
        }

        if (chars.IsEmpty)
        {
            chars.Clear();          // FN
        }

        if (readonlyChars.IsEmpty)
        {
            _ = readonlyChars[5];   // FN
        }
    }

    void ListPattern(byte[] bytes)
    {
        if (bytes is [])
        {
            bytes.Clone();          // FN
        }
    }
}
