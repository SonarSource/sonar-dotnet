public struct S
{
    public void LoopCounterChange((int, int) t)
    {
        for (int i = 0; i < 42; i++)
        {
            (i, var j) = t; // Noncompliant {{Do not update the loop counter 'i' within the loop body.}}
//           ^
            (_, i) = (1, 2); // Noncompliant {{Do not update the loop counter 'i' within the loop body.}}
            (_, (_, i, _)) = (1, (2, 3, 4)); // Noncompliant {{Do not update the loop counter 'i' within the loop body.}}
            (i, j) = (i, 2); // Noncompliant [issue1, issue2] FP
        }

        for (int i = 0, j = 0; i < 42; i++, j++)
        {
            (i, j) = (1, 2); // Noncompliant [issue3, issue4]
        }

        // loop variable shadowed in local function:
        for (int i = 0; i < 42; i++)
        {
            void M(int i)
            {
                (i, _) = (1, 2); // Compliant, this "i" is not a loop variable
            }

        }

        for (int i = 0; i < 42; i++)
        {
            int a = 10;
            (a, var j) = t;
        }
    }
}
