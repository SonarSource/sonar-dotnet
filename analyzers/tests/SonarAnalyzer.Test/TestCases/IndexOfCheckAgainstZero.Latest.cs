using System;

public class Sample
{
    public void Spans()
    {
        var span = new Span<char>("Hello".ToCharArray());
        var readonlySpan = new ReadOnlySpan<char>("Hello".ToCharArray());

        if (span.IndexOf('H') > 0) // FN
        {
            // ...
        }

        if (readonlySpan.IndexOf('H') > 0) // FN
        {
            // ...
        }
    }
}
