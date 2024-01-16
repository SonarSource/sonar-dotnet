using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class MyTestCases
    {
        public void Method1()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                throw new InvalidOperationException(); // Noncompliant
            }
        }

        public void Method2()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {

                }
                finally
                {
                    throw new InvalidOperationException(); // Noncompliant
                }
            }
        }

        public void Method3()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {
                    throw new InvalidOperationException(); // Noncompliant
                }
                catch (InvalidCastException)
                {

                }
            }
        }

        public void Method4()
        {
            int a = 0;
            try
            {
                a++;
                throw new ArgumentException();
            }
            finally
            {
                if (a > 0)
                {
                    throw new InvalidOperationException(); // Noncompliant
                }
            }
        }

        public void Method5()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {
                    try
                    {
                        throw new ArgumentOutOfRangeException(); // Noncompliant
                    }
                    finally
                    {
                        throw new InvalidOperationException(); // Noncompliant
                    }
                }
                catch
                {

                }
            }
        }

        public void Method6()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {
                    try
                    {
                        throw new InvalidOperationException(); // Noncompliant
                    }
                    catch
                    {

                    }

                    throw new ArgumentOutOfRangeException(); // Noncompliant
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw; // Noncompliant
                }
            }
        }

        public void Method7()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {
                    try
                    {
                        try
                        {
                            throw new InvalidOperationException(); // Noncompliant
                        }
                        catch
                        {

                        }

                        throw new ArgumentOutOfRangeException(); // Noncompliant
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw; // Noncompliant
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
        }

        public void Method8()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
            }
        }

        public void Method9()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {
                    throw new InvalidOperationException(); // Noncompliant
                }
                catch (InvalidOperationException)
                {

                }
            }
        }

        public void Method10()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {
                    throw new InvalidOperationException(); // Noncompliant
                }
                catch
                {

                }
            }
        }

        public void Method11()
        {
            try
            {
                throw new ArgumentException();
            }
            finally
            {
                try
                {
                    try
                    {
                        try
                        {
                            throw new InvalidOperationException(); // Noncompliant
                        }
                        catch
                        {

                        }

                        throw new ArgumentOutOfRangeException(); // Noncompliant
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw; // Noncompliant
                    }
                }
                catch
                {
                }
            }
        }
    }
}
