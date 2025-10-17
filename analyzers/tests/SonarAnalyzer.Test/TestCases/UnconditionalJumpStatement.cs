using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test1(string[] strings, int length)
        {
            for (int i = 0; i < length; i++)
            {
                break; // Noncompliant {{Refactor the containing loop to do more than one iteration.}}
//              ^^^^^^
            }

            for (int i = 0; i < length; i++)
            {
                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
            }

            for (int i = 0; i < length; i++)
            {
                return; // Noncompliant
            }

            for (int i = 0; i < length; i++)
            {
                throw new Exception(); // Noncompliant
            }

            foreach (var s in strings)
            {
                break; // Noncompliant
            }

            foreach (var s in strings)
            {
                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
            }

            foreach (var s in strings)
            {
                return; // Noncompliant
            }

            foreach (var s in strings)
            {
                throw new Exception(); // Noncompliant
            }

            while (true)
            {
                break; // Noncompliant
            }

            while (true)
            {
                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421 
            }

            while (true)
            {
                return; // Noncompliant
            }

            while (true)
            {
                throw new Exception(); // Noncompliant
            }

            do
            {
                break; // Noncompliant
            }
            while (true);

            do
            {
                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
            }
            while (true);

            do
            {
                return; // Noncompliant
            }
            while (true);

            do
            {
                throw new Exception(); // Noncompliant
            }
            while (true);
        }

        public void Test2(string[] strings, bool stop)
        {
            while (true)
            {
                if (stop)
                {
                    break; // Compliant
                }
            }

            while (true)
            {
                if (stop)
                {
                    continue; // Compliant
                }
            }

            while (true)
            {
                if (stop)
                {
                    return; // Compliant
                }
            }

            while (true)
            {
                if (stop)
                {
                    throw new Exception(); // Compliant
                }
            }
        }

        public void Test3(string[] strings, bool stop)
        {
            if (stop)
            {
                while (true)
                {
                    break; // Noncompliant
                }

                while (true)
                {
                    continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
                }

                while (true)
                {
                    return; // Noncompliant {{Refactor the containing loop to do more than one iteration.}}
//                  ^^^^^^^
                }

                while (true)
                {
                    throw new Exception(); // Noncompliant {{Refactor the containing loop to do more than one iteration.}}
//                  ^^^^^^^^^^^^^^^^^^^^^^
                }
            }
        }

        public void Test4(string[] strings, int padding)
        {
            while (true)
            {
                switch (padding)
                {
                    case 1:
                        break; // Compliant
                    case 2:
                        throw new Exception(); // Compliant
                    default:
                        break; // Compliant
                }
            }

            while (true)
            {
                switch (padding)
                {
                    case 1:
                        return; // Compliant
                    default:
                        return; // Compliant
                }
            }
        }

        public void Test5(Action doSomething, Action<Exception> logError)
        {
            while (true)
            {
                try
                {
                    doSomething();
                }
                catch (Exception e)
                {
                    logError(e);
                    throw; // Compliant
                }
            }
        }

        public Func<int> Test6 = () =>
        {
            if (true)
            {
                return 5;
            }
            else
            {
                return 10;
            }
        };

        public void Test7()
        {
            while (true)
            {
                if (Foo())
                {
                    continue;
                }

                break;
                GetHashCode();
            }
        }

        public bool Foo() { return true; }

        public void Test8()
        {
            while (true)
            {
                if (true)
                {
                    break;
                }

                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
            }
        }

        public void Test9()
        {
            while (true)
            {
                if (true)
                {
                    continue;
                }

                return; // Compliant
            }
        }

        public void Test10()
        {
            while (true)
            {
                if (true)
                {
                    throw new Exception();
                }

                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
            }
        }

        public void Test11()
        {
            while (true)
            {
                if (true)
                {
                    return;
                }

                break; // Noncompliant
            }
        }
        public void Test12()
        {
            while (true)
            {
                foo();
                bar();
                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
            }
        }
        public void Test13()
        {
            while (true)
            {
                foo();
                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
                bar();
            }
        }

        public void Test14()
        {
            while (true)
            {
                foo();
                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
                bar();
            }
        }

        public void Test15()
        {
            while (true)
            {
                UtilFunc(() =>
                {
                    return; // Compliant
                });

                continue; // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2421
            }
        }

        public void Test16()
        {
            while (true)
            {
                if (true)
                {
                    ;
                }
                else
                {
                    continue;
                }

                break; // Compliant
            }
        }

        public void Test17()
        {
            while (true)
            {
                while (true)
                {
                    if (true)
                    {
                        ;
                    }
                    else
                    {
                        break;
                    }
                }

                break; // Noncompliant
            }
        }

        void UtilFunc(Action a)
        {
        }

        public bool TestWithRetry(Action doSomething)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    doSomething();
                    return true; // Compliant
                }
                catch (Exception e)
                {
                    if (i == 2)
                    {
                        return false; // Compliant
                    }
                }
            }

            return false;
        }

        public bool TestWithRetry2()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return true; // Noncompliant
                }
                catch (Exception e)
                {
                    if (i == 2)
                    {
                        return false; // Compliant
                    }
                }
            }

            return false;
        }

        public bool TestWithRetry3(Func<bool> doSomething)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return (((doSomething()))); // Compliant
                }
                catch (Exception e)
                {
                    if (i == 2)
                    {
                        return false; // Compliant
                    }
                }
            }

            return false;
        }

        public bool TestWithRetry4(Action doSomething, Action<Exception> logError)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    doSomething();
                }
                catch (Exception e)
                {
                    logError(e);
                }
                finally
                {
                }
            }

            return true;
        }

        public async Task<bool> TestWithRetry5(Func<Task<bool>> doSomething)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return (((await doSomething()))); // Compliant
                }
                catch (Exception e)
                {
                    if (i == 2)
                    {
                        return false; // Compliant
                    }
                }
            }

            return false;
        }

        public void ResetRetryTimeout() { }
        public bool WaitRetryTimeout() { return true; }

        protected virtual FileStream CreateLock(string lockFileName, bool retries)
        {
            if (retries) ResetRetryTimeout();
            FileStream filelock = null;
            while (true)
            {
                try
                {
                    filelock = new FileStream(lockFileName,
                        FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.DeleteOnClose);
                    return filelock;
                }
                catch (Exception e)
                {
                    int code = System.Runtime.InteropServices.Marshal.GetHRForException(e);
                    if (code == unchecked((int)0x80070020) ||
                        code == unchecked((int)0x80070021))
                    {
                        // Sharing violation
                        if (!retries) return null;
                        if (!WaitRetryTimeout()) throw;
                    }
                    else
                    {
                        // All others are considered an error and we don't retry
                        throw;
                    }
                }
            };
        }

        public void foo() { }
        public void bar() { }
    }

    class ReproducerClass
    {
        ReproducerClass FirstProperty => null;
        int SecondProperty => throw new Exception();

        int MethodWithPropertyAccess()
        {
            while (true)
            {
                try
                {
                    // SecondProperty will throw, it will be caught and another iteration will happen
                    return SecondProperty; // Compliant
                }
                catch (Exception ex)
                {
                }
            }
        }

        int MethodWithMemberAccess()
        {
            while (true)
            {
                try
                {
                    // FirstProperty will be null and another iteration will happen
                    return FirstProperty.SecondProperty; // Compliant
                }
                catch (Exception ex)
                {
                }
            }
        }

        int MethodWithElementAccess(int[] array)
        {
            while (true)
            {
                try
                {
                    return (((array[0]))); // Compliant
                }
                catch (Exception ex)
                {
                }
            }
        }

        int MethodWithAssignmentInsideReturnStatement()
        {
            int x;
            while (true)
            {
                try
                {
                    return x = SecondProperty; // Compliant
                }
                catch (Exception ex)
                {
                }
            }
        }
    }

    public class LocalFunctions
    {
        public void LocalFunctionInLoop()
        {
            while(true)
            {
                void Throw()
                {
                    throw new InvalidOperationException("Message");
                }
            }
        }

        public void StaticLocalFunctionInLoop()
        {
            while (true)
            {
                static void Throw()
                {
                    throw new InvalidOperationException("Message");
                }
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2441
    class ReproducerWithDelegate
    {
        delegate int Delegate1();
        void myFunc()
        {
            while (true)
            {
                Delegate1 d2 = () => { return -1; }; // Compliant
            }
        }

        void myFunc2()
        {
            while (true)
            {
                Delegate1 d1 = delegate () { return -1; }; // Compliant
            }
        }

        Func<int> myFunc3()
        {
            while (true)
            {
                return (Func<int>)delegate () { return -1; }; // Noncompliant
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7987
    class Repro7987
    {
        void Test(List<object> items)
        {
            var newItems = new List<object>();
            foreach (var item in items)
            {
                LocalFunction(item);
                continue;                                   // Noncompliant FP

                void LocalFunction(object item)
                {
                    if (items.Count < 10)
                        newItems.Add(item);
                }
            }
        }
    }
}
