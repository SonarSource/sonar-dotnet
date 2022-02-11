using System;
using System.Threading;

namespace SpinLock_Type
{
    class Program
    {
        private bool condition;

        public void Method1()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // FN
            if (condition)
            {
                sl.Exit();
            }
        }

        public void Method2()
        {
            SpinLock sl = new SpinLock(true);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // FN
            if (condition)
            {
                sl.Exit(true);
            }
        }

        public void Method3()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(ref isAcquired);
            if (condition)
            {
                sl.Exit();
            }
        }

        public void Method4()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(42, ref isAcquired);
            if (condition)
            {
                sl.Exit();
            }
        }

        public void Method5()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(new TimeSpan(42), ref isAcquired);
            if (condition)
            {
                sl.Exit();
            }
        }

        public void Method6(string someString)
        {
            bool isAcquired = false;
            SpinLock sl = new SpinLock(false); ;
            sl.Enter(ref isAcquired);
            try
            {
                Console.WriteLine(someString.Length);
            }
            finally
            {
                sl.Exit();
            }
        }

        public void Method7()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // Compliant
            if (isAcquired)
            {
                sl.Exit();
            }
        }

        public void Method8()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(ref isAcquired);
            if (isAcquired)
            {
                sl.Exit();
            }
        }

        public void Method9()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // Compliant
            sl.Exit();
        }
    }
}
