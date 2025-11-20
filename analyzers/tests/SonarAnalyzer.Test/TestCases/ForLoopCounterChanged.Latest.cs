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
            (i, j) = (i, 2); // Noncompliant FP
        }

        for (int i = 0, j = 0; i < 42; i++, j++)
        {
            (i, j) = (1, 2); // Noncompliant [issue1, issue2]
        }

        // loop variable shadowed in local function:
        for (int i = 0; i < 42; i++)
        {
            void M(int i)
            {
                (i, _) = (1, 2); // Compliant, this "i" is not a loop variable
            }

        }

        // Loop variable shadowed by re-declaration.
        for (int i = 0; i < 42; i++)
        {
            var (i, j) = (1, 2);        // Error [CS0136] - FN - we still check for SonarLint as it analyzes also code with compile errors.
            _ = (1, 2) is var (i, b);   // Error [CS0128] - FN - we still check for SonarLint as it analyzes also code with compile errors.
        }

        for (var i = (a: 1, b: 2); i is (a: < 10, _); i = (++i.a, ++i.b))
        {
            i = (1, 1); // Noncompliant
            i.a = 1;    // FN
        }

        for (var (i, j, _) = (0, 0, 0); i < 10; ++i, ++j)
        {
            i = 0;  // Noncompliant
            _ = 0;  // Compliant. This discard does not refer to the other one.
        }

        for ((int i, int j, var _, _) = (0, 0, 0, 0); i < 10; ++i, ++j)
        {
            i = 0;  // Noncompliant
            _ = 0;  // Compliant. This discard does not refer to the other ones.
        }

        int k, l, m;
        for ((k, l) = (0, 0), m = 0; k < 10; ++k, ++l)
        {
            k = 0; // Noncompliant
            m = 0; // Noncompliant
        }

        for (int i = 0; i < 42; i++)
        {
            int a = 10;
            (a, var j) = t;
        }
    }

    public void LocalNamedAsDiscard()
    {
        int i, _;
        for ((i, _) = (0, 0); i < 10; ++i)
        {
            _ = 0; // Noncompliant. Here _ is not a discard but a local reference.
        }
    }
}

public class SomeClass  // https://sonarsource.atlassian.net/browse/NET-2658
{
    private int i;

    public void CSharp11Compound((int, int) t)
    {
        for (int i = 0; i < 42; i++)
        {
            i >>>= 1; // FN
        }
    }

    public void NullConditionalAssignment(SomeClass arg)
    {
        for (arg?.i = 0; arg.i < 3; arg.i++)
        {
            arg.i = 1; // FN
        }
    }
}

