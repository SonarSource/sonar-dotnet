using System;
using System.Threading;

namespace ReaderWriterLock_Type
{
    class Program
    {
        private bool condition;
        private ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        public void Method1()
        {
            readerWriterLock.AcquireReaderLock(42); // Noncompliant
            if (condition)
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void Method2()
        {
            readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // Noncompliant
            if (condition)
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void Method3()
        {
            readerWriterLock.AcquireWriterLock(42); // Noncompliant
            if (condition)
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public void Method4()
        {
            readerWriterLock.AcquireWriterLock(new TimeSpan(42)); // Noncompliant
            if (condition)
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public void Method5()
        {
            readerWriterLock.AcquireReaderLock(42);
            try
            {
                var cookie = readerWriterLock.UpgradeToWriterLock(42);
                if (condition)
                {
                    readerWriterLock.DowngradeFromWriterLock(ref cookie);
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void Method6()
        {
            try
            {
                readerWriterLock.AcquireReaderLock(new TimeSpan(42));
                var cookie = readerWriterLock.UpgradeToWriterLock(new TimeSpan(42));
                if (condition)
                {
                    readerWriterLock.DowngradeFromWriterLock(ref cookie);
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void Method7(string arg)
        {
            try
            {
                readerWriterLock.AcquireReaderLock(42);
                var cookie = readerWriterLock.UpgradeToWriterLock(42);
                try
                {
                    Console.WriteLine(arg.Length);
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    readerWriterLock.DowngradeFromWriterLock(ref cookie);
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void Method8(string arg)
        {
            try
            {
                readerWriterLock.AcquireReaderLock(new TimeSpan(42));
                var cookie = readerWriterLock.UpgradeToWriterLock(new TimeSpan(42));
                try
                {
                    Console.WriteLine(arg.Length);
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    readerWriterLock.DowngradeFromWriterLock(ref cookie);
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void Method9()
        {
            readerWriterLock.AcquireReaderLock(new TimeSpan(42)); // Noncompliant
            LockCookie cookie = new LockCookie();
            if (condition)
            {
                cookie = readerWriterLock.ReleaseLock();
            }

            readerWriterLock.RestoreLock(ref cookie);
        }

        public void Method10()
        {
            readerWriterLock.AcquireReaderLock(42); // Compliant
            readerWriterLock.ReleaseReaderLock();
        }

        public void WrongOrder()
        {
            readerWriterLock.ReleaseReaderLock();
            readerWriterLock.AcquireReaderLock(1);  // Compliant, source of FPs on Peach

            var a = new ReaderWriterLock();
            a.ReleaseLock();
            a.AcquireWriterLock(1);

            var b = new ReaderWriterLock();
            b.ReleaseWriterLock();
            b.AcquireWriterLock(1);
        }

        public void IsReaderLockHeld()
        {
            readerWriterLock.AcquireReaderLock(42);
            if (readerWriterLock.IsReaderLockHeld)  // Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void IsReaderLockHeld_NoLocking()
        {
            if (readerWriterLock.IsReaderLockHeld)    // Noncompliant
            {
                if (condition)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }
        }

        public void IsReaderLockHeld_NoLocking_Compliant()
        {
            if (readerWriterLock.IsReaderLockHeld)
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void IsReaderLockHeld_Noncompliant()
        {
            readerWriterLock.AcquireReaderLock(42);
            if (readerWriterLock.IsReaderLockHeld)  // Noncompliant
            {
                if (condition)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }
        }

        public void IsReaderLockHeld_Noncompliant(bool arg)
        {
            if (arg)
            {
                readerWriterLock.AcquireReaderLock(42);
            }
            if (readerWriterLock.IsReaderLockHeld)  // Noncompliant
            {
                if (condition)
                {
                    readerWriterLock.ReleaseReaderLock();
                }
            }
        }

        public void IsReaderLockHeld_Unreachable()
        {
            readerWriterLock.AcquireReaderLock(42);
            if (readerWriterLock.IsReaderLockHeld)  // Noncompliant, ends up unreleased on If path, and released on Else path
            {
                //
            }
            else
            {
                readerWriterLock.ReleaseReaderLock();
            }
        }

        public void IsReaderLockHeld_WriteLockReleased_OutOfScope()
        {
            if (readerWriterLock.IsReaderLockHeld)    // Noncompliant, this rule doesn't care about lock type mismatch
            {
                if (condition)
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
        }

        public void IsWriterLockHeld()
        {
            readerWriterLock.AcquireWriterLock(42);
            if (readerWriterLock.IsWriterLockHeld)   // Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            {
                readerWriterLock.ReleaseWriterLock();
            }
        }

        public void IsWriterLockHeld_InsideTryCatch()
        {
            try
            {
                readerWriterLock.AcquireWriterLock(42);
                if (readerWriterLock.IsWriterLockHeld)   // Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
            catch
            {
            }
        }

        public void IsWriterLockHeld_Noncompliant()
        {
            if (readerWriterLock.IsWriterLockHeld)  // Noncompliant
            {
                if (condition)
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
        }
    }
}
