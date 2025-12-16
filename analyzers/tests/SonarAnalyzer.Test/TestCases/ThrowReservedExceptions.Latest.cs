using System;

public class Sample
{
    public void NullCoalesce(object arg)
    {
        _ = arg ?? throw new Exception();   // Noncompliant
        //               ^^^^^^^^^^^^^^^

        _ = arg switch
        {
            string s => s,
            _ => throw new Exception()      // Noncompliant
            //         ^^^^^^^^^^^^^^^
        };
    }
}
