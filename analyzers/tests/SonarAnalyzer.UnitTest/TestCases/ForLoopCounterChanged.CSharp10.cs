public struct S
{
    public void LoopCounterChange((int, int) t)
    {
        for (int i = 0; i < 42; i++)
        {
            (i, var j) = t; // FN
        }
    }
}
