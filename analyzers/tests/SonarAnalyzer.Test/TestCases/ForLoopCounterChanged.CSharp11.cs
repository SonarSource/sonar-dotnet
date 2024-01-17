public class SomeClass
{
    public void LoopCounterChange((int, int) t)
    {
        for (int i = 0; i < 42; i++)
        {
            i >>>= 1; // FN
        }
    }
}
