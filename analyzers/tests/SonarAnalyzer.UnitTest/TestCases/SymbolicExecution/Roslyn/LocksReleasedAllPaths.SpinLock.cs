using System;
using System.Threading;

namespace SpinLock_Type
{
    class Program
    {
        private bool condition;

        public void Enter_PartialExit()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // Noncompliant
            if (condition)
            {
                sl.Exit();
            }
        }

        public void Enter_ThreadIdTrackingEnabled_PartialExit()
        {
            SpinLock sl = new SpinLock(true);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // Noncompliant
            if (condition)
            {
                sl.Exit(true);
            }
        }

        public void TryEnter_ThreadIdTrackingDisabled_PartialExit()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(ref isAcquired); // Noncompliant
            if (condition)
            {
                sl.Exit();
            }
        }

        public void TryEnterIntOverload_PartialExit()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(42, ref isAcquired); // Noncompliant
            if (condition)
            {
                sl.Exit();
            }
        }

        public void TryEnterTimeSpanOverload_PartialExit()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(new TimeSpan(42), ref isAcquired); // Noncompliant
            if (condition)
            {
                sl.Exit();
            }
        }

        public void TryCatchFinally_Compliant(string someString)
        {
            bool isAcquired = false;
            SpinLock sl = new SpinLock(false);
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

        public void Enter_UseReturnValueToReleaseOnlyWhenNeeded()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.Enter(ref isAcquired); // Noncompliant, FP
            if (isAcquired)
            {
                sl.Exit();
            }
        }

        public void TryEnter_UseReturnValueToReleaseOnlyWhenNeeded()
        {
            SpinLock sl = new SpinLock(false);
            bool isAcquired = false;
            sl.TryEnter(ref isAcquired); // Noncompliant, FP
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
