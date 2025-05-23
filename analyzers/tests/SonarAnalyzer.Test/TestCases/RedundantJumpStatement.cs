﻿using System;
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

        // https://github.com/SonarSource/sonar-dotnet/issues/1265
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

        void RedundantJumpInTryCatch1()
        {
            try
            {
                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine();
                return; // Noncompliant
            }
            finally
            {
                Console.WriteLine();
            }
        }

        void RedundantJumpInTryCatch2()
        {
            try
            {
                Console.WriteLine();
                return; // Noncompliant
            }
            catch (Exception)
            {
                Console.WriteLine();
            }
            finally
            {
                Console.WriteLine();
            }
        }

        void CSharp8_StaticLocalFunctions()
        {
            static void Compute(int a, out int b)
            {
                b = a;
                return;     // Noncompliant
            }
            static void EnsurePositive(int a, out int b)
            {
                b = 0;
                if (a <= 0)
                {
                    return;
                }
                b = a;
            }

        }
    }

    public interface IWithDefaultImplementation
    {
        decimal Count { get; set; }
        decimal Price { get; set; }

        //Default interface methods
        void Reset()
        {
            Price = 0;
            Count = 0;
            return;     // Noncompliant
        }

        void ResetIfZero()
        {
            if (Count == 0)
            {
                return;
            }
            Price = 0;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3105
    public class Repro_3105
    {
        public void DoSomething()
        {
            Action a = () => throw new Exception();

            Invoke(() => throw new Exception());
        }

        void Invoke(Action a)
        {
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3193
    public class Repro_3193
    {
        void Foo(Log Logger)
        {
            {
                var retryAttempts = 0;
                while (retryAttempts < 3)
                {
                    retryAttempts++;
                    try
                    {
                        // Do something real here.
                        return; // Noncompliant FP, this should end the loop if previous statement succeeded
                    }
                    catch (Exception ex)
                    {
                        //Log
                    }
                    finally
                    {
                        Logger?.Finally(); // Removing ? generates simple block and different CFG shape (Finally is on different location)
                    }
                }
            }
        }
    }

    public class Log
    {
        public void Finally() { }
    }
}
