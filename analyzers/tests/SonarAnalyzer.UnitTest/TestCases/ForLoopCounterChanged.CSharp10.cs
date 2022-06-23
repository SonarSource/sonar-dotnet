public struct S
{
    public void LoopCounterChange((int, int) t)
    {
        for (int i = 0; i < 42; i++)
        {
            (i, var j) = t; // Noncompliant {{Do not update the loop counter 'i' within the loop body.}}
//          ^^^^^^^^^^
        }

        for (int i = 0; i < 42; i++)
        {
            (i, var a, var b, int c) = (1, 2, 3, 4); // Noncompliant {{Do not update the loop counter 'i' within the loop body.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^
        }

        for (int i = 0; i < 42; i++)
        {
            int a = 10;
            (a, var j) = t;
        }
    }
}
