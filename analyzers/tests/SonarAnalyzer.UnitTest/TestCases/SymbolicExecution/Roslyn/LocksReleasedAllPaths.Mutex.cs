using System;
using System.Threading;

namespace Mutex_Type
{
    // https://docs.microsoft.com/en-us/dotnet/standard/threading/mutexes
    internal class Foo
    {
        bool cond;
        public Mutex instanceMutex;
        public static Mutex staticMutex;

        public void Noncompliant(Foo foo)
        {
            var m0 = new Mutex(true, "bar", out var m0WasCreated); // Noncompliant

            var m1 = new Mutex(false);
            m1.WaitOne(); // Noncompliant

            var m2 = new Mutex(false, "qix", out var m2WasCreated);
            m2.WaitOne(); // Noncompliant

            var m3 = Mutex.OpenExisting("x");
            m3.WaitOne(); // Noncompliant

            foo.instanceMutex.WaitOne(); // FN

            Foo.staticMutex.WaitOne(); // Noncompliant

            if (cond)
            {
                m0.ReleaseMutex();
                m1.ReleaseMutex();
                m2.ReleaseMutex();
                m3.ReleaseMutex();
                foo.instanceMutex.ReleaseMutex();
                Foo.staticMutex.ReleaseMutex();
            }

            // Note that Dispose() closes the underlying WaitHandle, but does not release the mutex
            m0.Dispose();
            m1.Dispose();
            m2.Dispose();
            m3.Dispose();
        }

        public void MutexWasCreatedAndReleased(bool arg)
        {
            var m = new Mutex(initiallyOwned: true, "bar", out var wasCreated); // Noncompliant
            if (wasCreated && arg)
            {
                m.ReleaseMutex();
            }
        }

        public void MutexWasCreatedAndNotReleased(bool arg)
        {
            var m = new Mutex(initiallyOwned: true, "bar", out var wasCreated); // Compliant
            if (!wasCreated && arg)
            {
                // wasCreated is false here, indicating that the lock is NOT (yet) held
                m.ReleaseMutex(); // This is a release without a previous lock. This is not covered by this rule.
            }
        }

        public void MutexWasCreatedWithoutOwnership(bool arg)
        {
            var m = new Mutex(initiallyOwned: false, "bar", out var wasCreated); // Compliant. The lock must still be acquired by requesting it.
            if (wasCreated && arg)
            {
                m.ReleaseMutex();
            }
        }

        public void Noncompliant2(Mutex paramMutex, Mutex paramMutex2)
        {
            // 'true' means it owns the mutex if no exception gets thrown
            using (var mutexInUsing = new Mutex(true, "foo")) // Noncompliant
            {
                if (cond)
                {
                    mutexInUsing.ReleaseMutex();
                }
            }

            if (Mutex.TryOpenExisting("y", out var mutexInOutVar))
            {
                mutexInOutVar.WaitOne(); // Noncompliant
                if (cond)
                {
                    mutexInOutVar.ReleaseMutex();
                }
            }

            var m = new Mutex(false);
            var mIsAcquired = m.WaitOne(200, true);
            if (mIsAcquired)
            {
                // here it should be released
            }
            else
            {
                m.ReleaseMutex(); // this is a programming error, but not subject of this rule
            }

            var paramMutexIsAcquired = paramMutex.WaitOne(400, false); // Noncompliant
            if (paramMutexIsAcquired)
            {
                if (cond)
                {

                }
                else
                {
                    paramMutex.ReleaseMutex();
                }
            }
            while (paramMutex2.WaitOne(400, false)) // Noncompliant
            {
                if (cond)
                {
                    paramMutex2.ReleaseMutex();
                }
            }
        }

        public void NoncompliantReleasedThenAcquiredAndReleased(Mutex paramMutex)
        {
            paramMutex.ReleaseMutex();
            paramMutex.WaitOne(); // Noncompliant
            if (cond)
            {
                paramMutex.ReleaseMutex();
            }
        }

        public void DifferentInstancesOnThis(Foo foo)
        {
            foo.instanceMutex.WaitOne(); // Compliant
            instanceMutex.WaitOne(); // Noncompliant
            if (cond)
            {
                instanceMutex.ReleaseMutex();
            }
        }

        public void DifferentInstancesOnParameter(Foo foo)
        {
            foo.instanceMutex.WaitOne(); // FN
            instanceMutex.WaitOne(); // Compliant
            if (cond)
            {
                foo.instanceMutex.ReleaseMutex();
            }
        }

        public void UnsupportedWaitAny(Mutex m1, Mutex m2, Mutex m3)
        {
            // it is too complex to support this scenario
            WaitHandle[] handles = new[] { m1, m2, m3 };
            var index = WaitHandle.WaitAny(handles); // FN
                                                     // the mutex at the given index should be released
            var acquiredMutex = (Mutex)handles[index];
            if (cond)
            {
                acquiredMutex.ReleaseMutex();
            }
        }

        public void UnsupportedWaitAll(Mutex m1, Mutex m2, Mutex m3)
        {
            // it is too complex to support this scenario
            WaitHandle[] handles = new[] { m1, m2, m3 };
            var allHaveBeenAcquired = WaitHandle.WaitAll(handles); // FN
            if (allHaveBeenAcquired)
            {
                // all indexes should be released
                if (cond)
                {
                    ((Mutex)handles[0]).ReleaseMutex();
                }
            }
        }

        public void CompliantAcquiredNotReleased(Mutex paramMutex, Foo foo)
        {
            using (var m = new Mutex(true, "foo"))
            {
                // do stuff
            }

            var m0 = new Mutex(true, "bar", out var m0WasCreated);
            m0.Dispose();

            var m1 = new Mutex(false);
            m1.WaitOne();
            m1.Dispose();

            var m2 = new Mutex(false, "qix", out var m2WasCreated);
            m2.WaitOne();
            m2.Dispose();

            var m3 = Mutex.OpenExisting("x");
            m3.WaitOne();

            if (Mutex.TryOpenExisting("y", out var m4))
            {
                m4.WaitOne();
            }

            var isAcquired = paramMutex.WaitOne(400, false);
            if (isAcquired)
            {
                // not released
            }

            foo.instanceMutex.WaitOne();
            Foo.staticMutex.WaitOne();
        }

        public void CompliantNotAcquired(Mutex paramMutex)
        {
            var m1 = new Mutex(false);
            var m2 = Mutex.OpenExisting("foo");
            if (Mutex.TryOpenExisting("foo", out var m3))
            {
                // do stuff but don't acquire
            }
            var m4 = new Mutex(false, "foo", out var mutexWasCreated);
            if (paramMutex != null)
            {
                // do stuff but don't acquire
            }
        }

        public void CompliantAcquiredAndReleased(Mutex paramMutex, Foo foo)
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
            if (paramMutex.WaitOne(400, false))
            {
                paramMutex.ReleaseMutex();
            }

            foo.instanceMutex.WaitOne();
            if (cond)
            {
                foo.instanceMutex.ReleaseMutex();
            }
            else
            {
                foo.instanceMutex.ReleaseMutex();
            }

            while (cond)
            {
                Foo.staticMutex.WaitOne();
                Foo.staticMutex.ReleaseMutex();
            }
        }

        public void ReleasedThenAcquired(Mutex paramMutex)
        {
            if (cond)
            {
                paramMutex.ReleaseMutex();
            }
            paramMutex.WaitOne(); // Compliant, source of FPs
        }

        public void CompliantComplex(string mutexName, bool shouldAcquire)
        {
            Mutex m = null;
            bool acquired = false;
            try
            {
                m = Mutex.OpenExisting(mutexName);
                if (shouldAcquire)
                {
                    m.WaitOne();    // Compliant, depends on tracking Null constraint for 'm'
                    acquired = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            finally
            {
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

        public void MutextAquireByConstructor_SimpleAssignment_LiteralArgument()
        {
            var m = new Mutex(true, "bar", out var mutextCreated); // Noncompliant
            if (cond)
            {
                m.ReleaseMutex();
            }
        }

        public void MutextAquireByConstructor_SimpleAssignment_Array()
        {
            Mutex[] arr = new Mutex[2];
            arr[0] = new Mutex(true, "bar", out var mutextCreated); // FN, untracked symbol
            if (cond)
            {
                arr[0].ReleaseMutex();
            }
        }

        public void MutextAquireByConstructor_SimpleAssignment_FieldArgument()
        {
            var m = new Mutex(cond, "bar", out var mutextCreated);
            if (cond)
            {
                m.ReleaseMutex();
            }
        }

        public void MutextAquireByConstructor_ReAssignment()
        {
            var m = new Mutex(false, "bar", out var mc1);
            m = new Mutex(true, "bar", out var mc2); // Noncompliant
            if (cond)
            {
                m.ReleaseMutex();
            }
        }

        public void MutextAquireByConstructor_MultipleVariableDeclaration()
        {
            Mutex m0, m1;
            m0 = m1 = new Mutex(true, "bar", out var mutextCreated); // FN

            if (cond)
            {
                m0.ReleaseMutex();
            }
        }

        public void MutextAquireByConstructor_SwitchExpression(int x)
        {
            var m = x switch
            {
                1 => new Mutex(),
                2 => new Mutex(new bool()),
                3 => new Mutex(true, "bar", out var mutextCreated), // FN
            };

            if (cond)
            {
                m.ReleaseMutex();
            }
        }

        public void NullTrackingCondition()
        {
            if (instanceMutex != null)
            {
                instanceMutex.WaitOne();    // Compliant, depends on tracking null conditions
            }


            if (instanceMutex != null)
            {
                instanceMutex.ReleaseMutex();
            }
        }
    }
}
