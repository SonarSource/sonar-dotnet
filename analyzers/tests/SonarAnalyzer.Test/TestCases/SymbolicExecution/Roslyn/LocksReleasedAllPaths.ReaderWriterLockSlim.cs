using System;
using System.Threading;

namespace ReaderWriterLockSlim_Type
{
    class Program
    {
        private bool condition;
        private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

        public void Method1()
        {
            readerWriterLockSlim.EnterReadLock(); // Noncompliant
            if (condition)
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void Method2()
        {
            readerWriterLockSlim.EnterWriteLock(); // Noncompliant
            if (condition)
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Method3()
        {
            readerWriterLockSlim.EnterUpgradeableReadLock(); // Noncompliant
            if (condition)
            {
                readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void TryEnterReadLock_Compliant()
        {
            if (readerWriterLockSlim.TryEnterReadLock(42)) // Compliant, never released in this method
            {
            }
            else
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void TryEnterReadLock_Noncompliant()
        {
            if (readerWriterLockSlim.TryEnterReadLock(42) && condition) // Noncompliant
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void Method5()
        {
            if (readerWriterLockSlim.TryEnterReadLock(new TimeSpan(42)) && condition) // Noncompliant
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void Method6()
        {
            if (readerWriterLockSlim.TryEnterWriteLock(42) && condition) // Noncompliant
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Method7()
        {
            if (readerWriterLockSlim.TryEnterWriteLock(new TimeSpan(42)) && condition) // Noncompliant
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Method8()
        {
            if (readerWriterLockSlim.TryEnterUpgradeableReadLock(42) && condition) // Noncompliant
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void Method9()
        {
            if (readerWriterLockSlim.TryEnterUpgradeableReadLock(new TimeSpan(42)) && condition) // Noncompliant
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }


        public void Method10()
        {
            try
            {
                readerWriterLockSlim.EnterUpgradeableReadLock();
                readerWriterLockSlim.EnterWriteLock(); // Compliant
                if (condition)
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void Method11(string arg)
        {
            try
            {
                readerWriterLockSlim.EnterUpgradeableReadLock();
                readerWriterLockSlim.EnterWriteLock();
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
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                readerWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void Method12()
        {
            readerWriterLockSlim.EnterReadLock(); // Compliant
            readerWriterLockSlim.ExitReadLock();
        }

        public void Method13()
        {
            readerWriterLockSlim.EnterReadLock(); // Compliant, this rule doesn't care if it was released with correct API
            readerWriterLockSlim.ExitWriteLock();
        }

        public void WrongOrder()
        {
            readerWriterLockSlim.ExitReadLock();
            readerWriterLockSlim.EnterReadLock(); // Compliant

            var a = new ReaderWriterLockSlim();
            a.ExitWriteLock();
            a.EnterWriteLock();

            var b = new ReaderWriterLockSlim();
            b.ExitUpgradeableReadLock();
            b.TryEnterReadLock(1);

            var c = new ReaderWriterLockSlim();
            c.ExitReadLock();
            c.TryEnterWriteLock(1);

            var d = new ReaderWriterLockSlim();
            d.ExitReadLock();
            d.EnterUpgradeableReadLock();

            var e = new ReaderWriterLockSlim();
            e.ExitReadLock();
            e.TryEnterUpgradeableReadLock(1);
        }

        public void Method14()
        {
            readerWriterLockSlim.EnterReadLock(); // Noncompliant, this rule doesn't care if it was released with correct API
            if (condition)
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Method15()
        {
            if (readerWriterLockSlim.TryEnterReadLock(42))
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void EarlyExit()
        {
            if (readerWriterLockSlim.TryEnterReadLock(0) == false) // Compliant https://github.com/SonarSource/sonar-dotnet/issues/5415
            {
                return;
            }

            try
            {
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void EarlyExit_UnaryOperator()
        {
            if (!readerWriterLockSlim.TryEnterReadLock(0)) // Compliant https://github.com/SonarSource/sonar-dotnet/issues/5415
            {
                return;
            }

            try
            {
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void Throw_Finally(object param)
        {
            readerWriterLockSlim.EnterReadLock(); // Compliant
            try
            {
                if (param == null)
                    throw new ObjectDisposedException("");

                return;
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void IsReadLockHeld()
        {
            readerWriterLockSlim.EnterReadLock();
            if (readerWriterLockSlim.IsReadLockHeld)    // Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void IsReadLockHeld_NoLocking()
        {
            if (readerWriterLockSlim.IsReadLockHeld)    // Noncompliant
            {
                if (condition)
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public void IsReadLockHeld_NoLocking_Compliant()
        {
            if (readerWriterLockSlim.IsReadLockHeld)
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void IsReadLockHeld_Noncompliant()
        {
            readerWriterLockSlim.EnterReadLock();
            if (readerWriterLockSlim.IsReadLockHeld)    // Noncompliant
            {
                if (condition)
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public void IsReadLockHeld_Noncompliant(bool arg)
        {
            if (arg)
            {
                readerWriterLockSlim.EnterReadLock();
            }
            if (readerWriterLockSlim.IsReadLockHeld)    // Noncompliant
            {
                if (condition)
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public void IsReadLockHeld_Unreachable()
        {
            readerWriterLockSlim.EnterReadLock();
            if (readerWriterLockSlim.IsReadLockHeld)    // Noncompliant, ends up unreleased on If path, and released on Else path
            {
                //
            }
            else
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void IsReadLockHeld_WriteLockReleased_OutOfScope()
        {
            if (readerWriterLockSlim.IsReadLockHeld)    // Noncompliant, this rule doesn't care about lock type mismatch
            {
                if (condition)
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public void IsWriteLockHeld()
        {
            readerWriterLockSlim.EnterWriteLock();  // Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
            if (readerWriterLockSlim.IsWriteLockHeld)
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void IsWriteLockHeld_InsideTryCatch()
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock();
                if (readerWriterLockSlim.IsWriteLockHeld)   // Compliant, https://github.com/SonarSource/sonar-dotnet/issues/5416
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
            catch
            {
            }
        }

        public void IsWriterLockHeld_Noncompliant()
        {
            if (readerWriterLockSlim.IsWriteLockHeld)  // Noncompliant
            {
                if (condition)
                {
                    readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public void IsUpgradeableReadLockHeld_Noncompliant()
        {
            if (readerWriterLockSlim.IsUpgradeableReadLockHeld) // Noncompliant
            {
                if (condition)
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public void IsReadLockHeld_UntrackedValue(Program untracked)
        {
            if (untracked.readerWriterLockSlim.IsReadLockHeld)    // We do not track fields of other instances
            {
                if (condition)
                {
                    readerWriterLockSlim.ExitReadLock();
                }
            }
        }

        public void OtherBoolProperty_Coverage(int? arg)
        {
            if (arg.HasValue)
            {
                readerWriterLockSlim.EnterReadLock();
                readerWriterLockSlim.ExitReadLock();
            }
        }

        public void SameProperty_AnotherType_Coverage()
        {
            var somethingElse = new SomethingElse();
            if (somethingElse.IsReadLockHeld)
            {
                if (condition)
                {
                    somethingElse.ExitReadLock();
                }
            }
        }

        private class SomethingElse
        {
            public bool IsReadLockHeld { get; }

            public void ExitReadLock() { }
        }
    }
}
