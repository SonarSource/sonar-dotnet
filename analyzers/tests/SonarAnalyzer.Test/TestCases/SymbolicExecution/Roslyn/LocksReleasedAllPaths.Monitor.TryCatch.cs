using System;
using System.Threading;

namespace Monitor_TryCatch
{
    class Program
    {
        private object obj = new object();

        public void Method1(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                Console.WriteLine(arg.Length);
            }
            catch (Exception ex)
            {
                Monitor.Exit(obj);
                throw;
            }
        }

        public void Method3(string arg)
        {
            Monitor.Enter(obj); // Compliant
            try
            {
                Console.WriteLine(arg.Length);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public void Method4(bool condition)
        {
            Monitor.Enter(obj); // Noncompliant

            if (condition)
            {
                throw new Exception();
            }

            Monitor.Exit(obj);
        }

        public void Method5(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                Console.WriteLine(arg.Length);
            }
            catch (NullReferenceException nre)
            {
                Monitor.Exit(obj);
                throw;
            }
            Monitor.Exit(obj);
        }

        public void CatchWhen(bool condition)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                Console.WriteLine();
                Monitor.Exit(obj);
            }
            catch when (condition)
            {
                Monitor.Exit(obj);
                throw;
            }
        }

        public void Method6(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                Console.WriteLine(arg.Length);  // throws NRE and UnknownException
            }
            catch (NullReferenceException nre) when (nre.Message.Contains("Dummy string"))
            {
                Monitor.Exit(obj);
                throw;
            }
        }

        public void CatchWhen_SpecificException(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                throw new NotImplementedException();
            }
            catch (NotImplementedException nre) when (nre.Message.Contains("Dummy string"))
            {
                Monitor.Exit(obj);
                throw;
            }
        }

        public void Method7(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                Console.WriteLine(arg.Length);
            }
            catch (Exception ex) when (ex is NullReferenceException)
            {
                Monitor.Exit(obj);
                throw;
            }
        }

        public void Method8(string arg)
        {
            Monitor.Enter(obj); // Compliant
            try
            {
                Console.WriteLine(arg.Length);
                Monitor.Exit(obj);
            }
            catch (Exception)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method9(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                Console.WriteLine(arg.Length);  // Can throw NullReferenceException when arg is null
                Monitor.Exit(obj);
            }
            catch (InvalidOperationException ex)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method10(string arg)
        {
            Monitor.Enter(obj); // Compliant
            try
            {
                Console.WriteLine(arg.Length);
                Monitor.Exit(obj);
            }
            catch
            {
                Monitor.Exit(obj);
            }
        }

        public void Method11(string arg)
        {
            Monitor.Enter(obj); // Compliant
            try
            {
                Console.WriteLine(arg.Length);
                Monitor.Exit(obj);
            }
            catch (NullReferenceException nre)
            {
                Monitor.Exit(obj);
            }
            catch (Exception)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method12(string arg)
        {
            Monitor.Enter(obj); // Noncompliant
            try
            {
                Console.WriteLine(arg.Length);
                Monitor.Exit(obj);
            }
            catch (NullReferenceException nre)
            {
            }
            catch (Exception)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method13(string arg)
        {
            Monitor.Enter(obj);

            try
            {
                throw new InvalidOperationException();
            }
            catch (InvalidOperationException)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method14(string arg)
        {
            Monitor.Enter(obj); // FN

            try
            {
                throw new NotImplementedException();
            }
            catch (InvalidOperationException)
            {
                Monitor.Exit(obj);
            }
        }

        public void Finally_Simple()
        {
            Monitor.Enter(obj);     // Compliant
            try
            {
                Console.WriteLine("CanThrow");
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public void Finally_Nested()
        {
            Monitor.Enter(obj);     // Compliant
            try
            {
                Console.WriteLine("Can throw");
                try
                {
                    Console.WriteLine("Can also throw");
                }
                finally
                {
                    Console.WriteLine("Can also throw from finally when disposing something");
                }
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public void Finally_Nested_ReleasedInWrongFinally()
        {
            Monitor.Enter(obj);     // Noncompliant
            try
            {
                Console.WriteLine("Can throw");
                try
                {
                    Console.WriteLine("Can also throw");
                }
                finally
                {
                    Monitor.Exit(obj);  // Wrong place
                }
            }
            finally
            {
                Console.WriteLine("Lock should be released here");
            }
        }


        public void Finally_Foreach(int[] values)
        {
            Monitor.Enter(obj);     // Compliant
            try
            {
                Console.WriteLine("CanThrow");
                foreach (var value in values)    // Produces implicit try/finally
                {
                    Console.WriteLine(value);   // Can throw
                }
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public void Finally_NestedInOuterCatch()
        {
            try
            {
                Monitor.Enter(obj);     // Compliant
                try
                {
                    Console.WriteLine("CanThrow");
                }
                finally
                {
                    Monitor.Exit(obj);
                }
            }
            catch (Exception ex)
            {
                // Handle it
            }
        }

        public void Finally_DoubleNestedInOuterCatch()
        {
            try
            {
                Monitor.Enter(obj);     // Compliant
                try
                {
                    try
                    {
                        Console.WriteLine("CanThrow");
                    }
                    finally
                    {
                        // Do nothing
                    }
                }
                finally
                {
                    Monitor.Exit(obj);
                }
            }
            catch (Exception ex)
            {
                // Handle it
            }
        }

        public void TryCatch_IsEntered_Compliant()
        {
            try
            {
                Monitor.Enter(obj);
                if (Monitor.IsEntered(obj)) // Compliant
                {
                    Monitor.Exit(obj);
                }
            }
            catch
            {
            }
        }

        public void TryCatch_IsEntered_Acquired(bool condition)
        {
            try
            {
                if (Monitor.IsEntered(obj)) // Noncompliant
                {
                    if (condition)
                    {
                        Monitor.Exit(obj);
                    }
                }
            }
            catch
            {
            }
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/5916
        public void TryFinally_NestedCfg()
        {
            Monitor.Enter(obj);
            try
            {
                Action a = () =>
                {
                    obj.ToString();
                    Monitor.Enter(obj);
                    Monitor.Exit(obj);
                };
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/7826
        public void Repro_7826(ReaderWriterLockSlim rwLock, object syncLock)
        {
            rwLock.EnterReadLock();
            rwLock.ExitReadLock();
            lock (syncLock)    // Noncompliant FP
            {
                Console.WriteLine();
            }
        }
    }
}
