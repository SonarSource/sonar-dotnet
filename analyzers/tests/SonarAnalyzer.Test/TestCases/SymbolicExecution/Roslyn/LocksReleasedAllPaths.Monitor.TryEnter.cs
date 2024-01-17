using System;
using System.Diagnostics;
using System.Threading;

namespace Monitor_TryEnter
{
    class Program
    {
        private object obj = new object();
        private bool condition;

        public void TryEnter_ExitedInElse()
        {
            if (Monitor.TryEnter(obj)) // Compliant, never exited in this method. It's a programming error not covered by this rule.
            {
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void TryEnter_ExitedInIf()
        {
            if (Monitor.TryEnter(obj)) // Noncompliant
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void TryEnter_Compliant()
        {
            if (Monitor.TryEnter(obj))
            {
                Monitor.Exit(obj);
            }
            else
            {
            }
        }

        public void TryEnter_WithInt()
        {
            if (Monitor.TryEnter(obj, 42)) // Noncompliant
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void TryEnter_WithRefIsAcquired(bool condition)
        {
            bool isAcquired = false;
            Monitor.TryEnter(obj, 42, ref isAcquired); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void TryEnter_WithTimeSpan()
        {
            if (Monitor.TryEnter(obj, new TimeSpan(42))) // Noncompliant
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void Method6(bool condition)
        {
            bool isAcquired = false;
            Monitor.TryEnter(obj, new TimeSpan(42), ref isAcquired); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void TryEnter_WithVariable_Compliant()
        {
            bool isAcquired = Monitor.TryEnter(obj, 42); // Compliant, never exited in this method

            if (isAcquired)
            {
            }
            else
            {
                Monitor.Exit(obj);
            }
        }

        public void TryEnter_WithVariable_Noncompliant()
        {
            bool isAcquired = Monitor.TryEnter(obj, 42); // Noncompliant

            if (isAcquired)
            {
                if (condition)
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void Method8()
        {
            bool isAcquired = Monitor.TryEnter(obj, 42);

            if (isAcquired)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method9()
        {
            Monitor.TryEnter(obj); // Compliant
            Monitor.Exit(obj);
        }

        public void Method10()
        {
            Monitor.TryEnter(obj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
            if(condition)
                Monitor.Exit(obj);
        }

        public void Method11()
        {
            switch (Monitor.TryEnter(obj))
            {
                case true:
                    Monitor.Exit(obj);
                    break;
                default:
                    break;
            }
        }

        public void TryEnter_Switch_Compliant()
        {
            switch (Monitor.TryEnter(obj)) // Compliant, never exited in this method
            {
                case false:
                    Monitor.Exit(obj);
                    break;
                default:
                    break;
            }
        }

        public void TryEnter_Switch_Noncompliant()
        {
            switch (Monitor.TryEnter(obj)) // Noncompliant
            {
                case false:
                    break;
                default:
                    if (condition)
                    {
                        Monitor.Exit(obj);
                    }
                    break;
            }
        }

        public void Method13(bool condition)
        {
            bool isAcquired = false;
            Monitor.TryEnter(obj, 42, ref isAcquired);
            if (isAcquired)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method14(bool condition)
        {
            bool isAcquired = false;
            Monitor.TryEnter(obj, ref isAcquired); // Noncompliant
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }

        public void Method15(bool condition)
        {
            bool isAcquired = false;
            Monitor.TryEnter(obj, ref isAcquired);
            if (isAcquired)
            {
                Monitor.Exit(obj);
            }
        }

        public void TryEnterInsideIf_Finally()
        {
            if (Monitor.TryEnter(obj, 500)) // Compliant https://github.com/SonarSource/sonar-dotnet/issues/5415
            {
                try
                {
                }
                finally
                {
                    Monitor.Exit(obj);
                }
            }
        }

        public void TryEnterInsideIf_Finally_WithVar()
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(obj); // Compliant https://github.com/SonarSource/sonar-dotnet/issues/5415
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        public void TryEnter_EarlyExit()
        {
            if (Monitor.TryEnter(obj) == false)
                return;

            try
            {
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }
    }
}
