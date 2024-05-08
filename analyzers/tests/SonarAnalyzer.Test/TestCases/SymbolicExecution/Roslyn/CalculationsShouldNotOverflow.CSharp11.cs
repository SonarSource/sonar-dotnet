using System;

public class Sample
{
    public void ListPattern(int[] array)
    {
        if (array is [2147483600 and var big, ..])
            _ = big + 100;  // FN
    }
}
