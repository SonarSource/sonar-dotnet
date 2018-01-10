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

        void Foo2()
        {
            if (a)
            {
                return; // Noncompliant
            }
            else
            {
                return;
                Foo2();
            }
        }

        void Loop_Continue()
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
                    Foo2();
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
    }
}
