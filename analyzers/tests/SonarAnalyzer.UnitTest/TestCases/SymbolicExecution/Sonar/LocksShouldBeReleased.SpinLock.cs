using System;
using System.Threading;

namespace Tests.Diagnostics
{
    class Program
    {
        public void DoSomethingWithSpinLock1(bool b)
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // FN
            if (b)
            {
                sl.Exit();
            }
        }

        public void DoSomethingWithSpinLock2(bool b)
        {
            SpinLock sl = new SpinLock(true);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // FN
            if (b)
            {
                sl.Exit(true);
            }
        }

        public void DoSomethingWithSpinLock3(bool b)
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(ref isAcquired); // FN
            if (b)
            {
                sl.Exit();
            }
        }

        public void DoSomethingWithSpinLock4(bool b)
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(42, ref isAcquired); // FN
            if (b)
            {
                sl.Exit();
            }
        }

        public void DoSomethingWithSpinLock5(bool b)
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(new TimeSpan(42), ref isAcquired); // FN
            if (b)
            {
                sl.Exit();
            }
        }

        public void DoSomethingWithSpinLock6(string b)
        {
            bool isAcquired = false;
            SpinLock sl = new SpinLock(false);;
            sl.Enter(ref isAcquired);
            try
            {
                Console.WriteLine(b.Length);
            }
            finally
            {
                sl.Exit();
            }
        }
    }
}
