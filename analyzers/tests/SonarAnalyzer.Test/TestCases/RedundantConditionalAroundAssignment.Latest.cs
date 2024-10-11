using System;

public class SomeClass
{
    public void Noncompliant(byte[] bytes)
    {
        if (bytes is [not 1, .., not 3]) // FN (is expression not supported yet)
        {
            bytes[0] = 1;
            bytes[^1] = 3;
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-443
    void IndexAccess()
    {
        DataBuffer buffer = new()
        {
            [-1] = -1,
            [1] = 0,
            [2] = 1,
            [3] = 2,
            [4] = 3,
            [5] = 4,
            [6] = 5,
            [7] = 6,
            [8] = 7,
            [9] = 8
        };
        if (buffer[^1] != 9)
        {
            buffer[^1] = 9; // FN
        }
    }
}

public class DataBuffer
{
    public int this[Index index]
    {
        get => 1;
        set { /* not relevant */ }
    }
}
