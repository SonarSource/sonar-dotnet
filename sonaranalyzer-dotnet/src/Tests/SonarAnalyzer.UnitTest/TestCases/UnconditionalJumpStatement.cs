using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test1(string[] strings)
        {
            for (int i = 0; i < length; i++)
            {
                break; // Noncompliant {{Remove this 'break' statement or make it conditional.}}
//              ^^^^^^
            }

            for (int i = 0; i < length; i++)
            {
                continue; // Noncompliant
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
                continue; // Noncompliant
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
                continue; // Noncompliant
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
                continue; // Noncompliant
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
                    continue; // Noncompliant
                }

                while (true)
                {
                    return; // Noncompliant {{Remove this 'return' statement or make it conditional.}}
//                  ^^^^^^^
                }

                while (true)
                {
                    throw new Exception(); // Noncompliant {{Remove this 'throw' statement or make it conditional.}}
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
                if (foo())
                {
                    continue;
                }

                break;
                GetHashCode();
            }
        }

        public void Test8()
        {
            while (true)
            {
                if (true)
                {
                    break;
                }

                continue; // Noncompliant
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

                continue; // Noncompliant
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
                continue; // Noncompliant
            }
        }
        public void Test13()
        {
            while (true)
            {
                foo();
                continue; // Noncompliant
                bar();
            }
        }

        public void Test14()
        {
            while (true)
            {
                foo();
                continue; // Noncompliant
                bar();
            }
        }

        public void Test15()
        {
            while (true)
            {
                UtilFunc(lambda =>
                {
                    return; // Compliant
                });

                continue; // Noncompliant
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
    }
}
