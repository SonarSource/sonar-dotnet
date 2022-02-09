using System;
using System.Collections.Generic;
using System.Threading;

namespace Monitor_Loops
{
    class Program
    {
        private object obj = new object();

        public void Method1()
        {
            Monitor.Enter(obj);     // Noncompliant tricky FP, as the execution should always reach number 9, but we don't track that
            for (int i = 0; i < 10; i++)
            {
                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void Method2()
        {
            Monitor.Enter(obj); // FN
            for (int i = 0; i < 10; i++)
            {
                break;
                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void Method3()
        {
            Monitor.Enter(obj); // Noncompliant
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
        }

        public void Method4()
        {
            Monitor.Enter(obj); // Noncompliant tricky FP, as the execution should always reach number 9, but we don't track that
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
        }

        public void Method5()
        {
            Monitor.Enter(obj); // Noncompliant
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
    }
}
