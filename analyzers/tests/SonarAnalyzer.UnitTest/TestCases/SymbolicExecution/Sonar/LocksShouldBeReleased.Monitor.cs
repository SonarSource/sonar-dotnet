using System;
using System.Threading;

namespace Tests.Diagnostics
{
    class Program
    {
        private object obj = new object();

        public void DoSomethingWithMonitor1(bool b)
        {
            Monitor.Enter(obj); // FN
            if (b)
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor2(bool b)
        {
            Monitor.Enter(obj); // FN
            switch (b)
            {
                case true:
                    Monitor.Exit(obj);
                    break;
                default:
                    break;
            }
        }

        public void DoSomethingWithMonitor3()
        {
            Monitor.Enter(obj); // FN
            var a = new Action(() =>
            {
                Monitor.Exit(obj);
            });
        }

        public int MyProperty
        {
            set
            {
                Monitor.Enter(obj); // FN
                if (value == 42)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void DoSomethingWithMonitor4()
        {
            Monitor.Enter(obj); // FN
            for (int i = 0; i < 10; i++)
            {
                if (i == 9)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void DoSomethingWithMonitor5(string b)
        {
            Monitor.Enter(obj); // FN
            try
            {
                Console.WriteLine(b.Length);
            }
            catch (Exception)
            {
                Monitor.Exit(obj);
                throw;
            }
        }

        public void DoSomethingWithMonitor6()
        {
            if (Monitor.TryEnter(obj))// FN
            {
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor7()
        {
            if (Monitor.TryEnter(obj)) // Compliant
            {
                Monitor.Exit(obj);
            }
            else
            {
            }
        }

        public void DoSomethingWithMonitor8(string b)
        {
            Monitor.Enter(obj); // Compliant
            Console.WriteLine(b.Length);
            Monitor.Exit(obj);
        }

        public void DoSomethingWithMonitor9(string b)
        {
            Monitor.Enter(obj); // Compliant
            try
            {
                Console.WriteLine(b.Length);
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor10(bool b)
        {
            bool isAcquired = false;
            Monitor.Enter(obj, ref isAcquired); // FN
            if (b)
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor11()
        {
            if (Monitor.TryEnter(obj, 42))// FN
            {
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor12(bool b)
        {
            bool isAcquired = false;
            Monitor.TryEnter(obj, 42, ref isAcquired); // FN
            if (b)
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor13()
        {
            if (Monitor.TryEnter(obj, new TimeSpan(42)))// FN
            {
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor14(bool b)
        {
            bool isAcquired = false;
            Monitor.TryEnter(obj, new TimeSpan(42), ref isAcquired); // FN
            if (b)
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor15()
        {
            bool isAcquired = Monitor.TryEnter(obj, 42); // FN

            if (isAcquired)
            {
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor16()
        {
            bool isAcquired = Monitor.TryEnter(obj, 42); // Compliant

            if (isAcquired)
            {
                Monitor.Exit(obj);
            }
        }

        public void DoSomethingWithMonitor17(bool b)
        {
            Monitor.Enter(obj); // FN

            if (b)
            {
                throw new Exception();
            }

            Monitor.Exit(obj);
        }
    }
}
