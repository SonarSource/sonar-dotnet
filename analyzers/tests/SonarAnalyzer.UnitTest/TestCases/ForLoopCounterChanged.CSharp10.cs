public struct S
{
    public void LoopCounterChange((int, int) t)
    {
        for (int i = 0; i < 42; i++)
        {
            (i, var j) = t; // Compliant - FN {{Do not update the loop counter 'i' within the loop body.}}
        }
    }
}
