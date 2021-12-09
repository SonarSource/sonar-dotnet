using System;
using System.Threading;

namespace Net6
{
    // https://docs.microsoft.com/en-us/dotnet/standard/threading/mutexes
    internal class NetMutexTest
    {
        public static void AcquiredNotReleased(Mutex paramMutex)
        {
            // Note that dispose does not release the resource

            using (var m = new Mutex(true, "foo")) // FN, 'true' means it owns the mutex if no exception gets thrown
            {
                // do stuff
            }

            var m0 = new Mutex(initiallyOwned: true, "bar", out var m0WasCreated); // FN
            m0.Dispose();

            var m1 = new Mutex(false);
            m1.WaitOne(); // FN
            m1.Dispose();

            var m2 = new Mutex(false, "qix", out var m2WasCreated);
            m2.WaitOne(); // FN
            m2.Dispose();

            var m3 = Mutex.OpenExisting("x");
            m3.WaitOne(); // FN

            if (Mutex.TryOpenExisting("y", out var m4))
            {
                m4.WaitOne(); // FN
            }

            var m5 = new Mutex(false);
            var acquired = m5.WaitOne(200, true); // FN, not released on the corect flow
            if (acquired)
            {
                // here it should be released
            }
            else
            {
                m5.ReleaseMutex(); // this is a programming error
            }

            var isAcquired = paramMutex.WaitOne(400, false); // FN
            if (isAcquired)
            {
                // not released
            }
        }

        public static void UnsupportedWaitAny(Mutex m1, Mutex m2, Mutex m3)
        {
            // it is too complex to support this scenario
            WaitHandle[] handles = new[] { m1, m2, m3 };
            var index = WaitHandle.WaitAny(handles);
            // the mutex at the given index should be released
        }

        public static void UnsupportedWaitAll(Mutex m1, Mutex m2, Mutex m3)
        {
            // it is too complex to support this scenario
            WaitHandle[] handles = new[] { m1, m2, m3 };
            var allHaveBeenAcquired = WaitHandle.WaitAll(handles);
            if (allHaveBeenAcquired)
            {
                // all indexes should be released
            }
        }

        public static void CompliantNotAcquired(Mutex paramMutex)
        {
            var m1 = new Mutex(false);
            var m2 = Mutex.OpenExisting("foo");
            if (Mutex.TryOpenExisting("foo", out var m3))
            {
                // do stuff but don't acquire
            }
            var m4 = new Mutex(initiallyOwned: false, "foo", out var mutexWasCreated);
            if (paramMutex != null)
            {
                // do stuff but don't acquire
            }
        }

        public static void CompliantAcquiredAndReleased(Mutex paramMutex)
        {
            var m1 = new Mutex(false);
            m1.WaitOne();
            m1.ReleaseMutex();

            var m2 = Mutex.OpenExisting("foo");
            if (m2.WaitOne(500))
            {
                m2.ReleaseMutex();
            }

            var isAcquired = paramMutex.WaitOne(400, false);
            if (isAcquired)
            {
                paramMutex.ReleaseMutex();
            }
        }

        public static void CompliantComplex(string mutexName, bool shouldAcquire)
        {
            Mutex m = null;
            bool acquired = false;
            try
            {
                m = Mutex.OpenExisting(mutexName);
                if (shouldAcquire)
                {
                    m.WaitOne();
                    acquired = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            finally
            {
                // there can be other exceptions when opening
                if (m != null)
                {
                    // can enter also if an exception was thrown when Waiting
                    if (acquired)
                    {
                        m.ReleaseMutex();
                    }
                    m.Dispose();
                }
            }
        }
    }
}
