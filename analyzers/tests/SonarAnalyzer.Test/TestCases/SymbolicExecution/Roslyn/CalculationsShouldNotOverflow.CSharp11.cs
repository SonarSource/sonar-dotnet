using System;

public class Sample
{
    public void ListPattern(int[] array)
    {
        if (array is [2147483600, ..])
            _ = array[0] + 100; // FN
    }
}
