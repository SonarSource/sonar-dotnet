using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantJumpStatement
    {
        void Foo1()
        {
            var a = new Action(() =>
            {
                return; // Noncompliant
//              ^^^^^^^
            });

            goto A; // Noncompliant
            A:
            return; // Noncompliant {{Remove this redundant jump.}}
        }

        int Prop
        {
            set
            {
                goto A; // Noncompliant
                A:
                return; // Noncompliant
            }
        }

        void Foo2(bool a)
        {
            if (a)
            {
                return; // Noncompliant
            }
            else
            {
                return;
                Foo2(a);
            }
        }

        void Loop_Continue(bool a)
        {
            for (int i = 0; i < 10; i++)
            {
                if (a)
                {
                    continue; // Noncompliant
                }
                else
                {
                    continue;
                    Foo2(a);
                }
            }
        }

        void Switch_Goto(int j)
        {
            switch (j)
            {
                case 1:
                    goto default; // Not reported
                default:
                    break;
            }

            throw new Exception();
        }

        void Switch_Return(int j)
        {
            switch (j)
            {
                case 1:
                    return; // Compliant
                case 2:
                    return; // Non-compliant, not reported
                    break;
            }
        }

        IEnumerable<int> YieldBreak1(int j)
        {
            yield break; // Compliant
        }

        IEnumerable<int> YieldReturn(int j)
        {
            yield return 1;
        }

        IEnumerable<int> YieldBreak2(int j)
        {
            yield return 1;
            yield break; // Noncompliant
        }

        void LongChain()
        {
            if (true)
            {
            }
            else if (true)
            {
                return; // Noncompliant
            }
            else if (false)
            { }
            else if (false)
            { }
            else
            { }
        }

        // https://github.com/SonarSource/sonar-csharp/issues/1265
        void RegressionTest_1265()
        {
            try
            {
                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine();
                return;
            }
            finally
            {
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
