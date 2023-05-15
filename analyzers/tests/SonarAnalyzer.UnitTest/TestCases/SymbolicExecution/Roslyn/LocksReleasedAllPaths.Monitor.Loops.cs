using System;
using System.Collections.Generic;
using System.Threading;

namespace Monitor_Loops
{
    class Program
    {
        private object obj = new object();
        private object other = new object();
        private bool condition;

        public void Method1()
        {
            Monitor.Enter(obj);     // Noncompliant tricky FP, as the execution should always reach number 9, but we don't track that
            if (condition)
            {
                Monitor.Exit(obj);  // To release it on at least one path to activate the rule
            }
            for (int i = 0; i < 10; i++)
            {
                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
            Monitor.Enter(other);   // Noncompliant
            if (condition) {
                Monitor.Exit(other);
            }
        }

        public void Method2()
        {
            Monitor.Enter(obj); // Compliant, because the lock is not released on any path
            for (int i = 0; i < 10; i++)
            {
                break;
                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
            Monitor.Enter(other);   // Noncompliant, to make sure we explore paths after the loop
            if (condition)
            {
                Monitor.Exit(other);
            }
        }

        public void Method3()
        {
            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);  // To release it on at least one path to activate the rule
            }
            for (int i = 0; i < 10; i++)
            {
                if (i == 5)
                {
                    break;
                }

                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
            Monitor.Enter(other);   // Noncompliant
            if (condition)
            {
                Monitor.Exit(other);
            }
        }

        public void Method4()
        {
            Monitor.Enter(obj); // Noncompliant tricky FP, as the execution should always reach number 9, but we don't track that
            if (condition)
            {
                Monitor.Exit(obj);  // To release it on at least one path to activate the rule
            }
            for (int i = 0; i < 10; i++)
            {
                if (i == 10)
                {
                    break;
                }

                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
            Monitor.Enter(other);   // Noncompliant
            if (condition)
            {
                Monitor.Exit(other);
            }
        }

        public void Method5()
        {
            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);  // To release it on at least one path to activate the rule
            }
            for (int i = 0; i < 10; i++)
            {
                if (i == 9)
                {
                    continue;
                }

                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
            Monitor.Enter(other);   // Noncompliant
            if (condition)
            {
                Monitor.Exit(other);
            }
        }

        public void Method6(bool condition, byte[] array)
        {
            Monitor.Enter(obj); // Noncompliant
            foreach (var item in array)
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void Method7(bool condition, byte[] array)
        {
            Monitor.Enter(obj); // Noncompliant, array can be empty
            foreach (var item in array)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method8(bool condition, List<byte> array)
        {
            Monitor.Enter(obj); // Noncompliant
            while (array.Count < 42)
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
                array.RemoveAt(0);
            }
        }

        public void Method9(bool condition, List<byte> array)
        {
            Monitor.Enter(obj); // Noncompliant, count can be bigger than 42
            while (array.Count < 42)
            {
                Monitor.Exit(obj);
                array.RemoveAt(0);
            }
        }

        public void Method10(bool condition, List<byte> array)
        {
            Monitor.Enter(obj); // Noncompliant
            do
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
                array.RemoveAt(0);
            }
            while (array.Count < 42);
        }

        public void Method11(bool condition, List<byte> array)
        {
            Monitor.Enter(obj); // Compliant
            do
            {
                Monitor.Exit(obj);
                array.RemoveAt(0);
            }
            while (array.Count < 42);
        }

        public void For_CanExit_PreIncrement()
        {
            for (var i = 0; i < 10; ++i)
            {
                Console.WriteLine();
            }

            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void For_CanExit_PostIncrement()
        {
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine();
            }

            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void For_CanExit_PreDecrement()
        {
            for (var i = 10; i > 0; --i)
            {
                Console.WriteLine();
            }

            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void For_CanExit_PostDecrement()
        {
            for (var i = 10; i > 0; i--)
            {
                Console.WriteLine();
            }

            Monitor.Enter(obj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }
    }
}
