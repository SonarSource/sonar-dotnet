using System;

public class Sample
{
    public void NullCoalesce(object arg)
    {
        _ = arg ?? throw new Exception(); // FN
    }
}
