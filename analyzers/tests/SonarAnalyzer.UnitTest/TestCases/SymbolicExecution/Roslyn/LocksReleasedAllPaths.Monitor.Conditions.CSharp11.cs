using System;
using System.Threading;

namespace Monitor_Conditions_CSharp11
{
    interface IInterface
    {
        static virtual void Method1(bool condition)
        {
            var lockObj = new Object();

            Monitor.Enter(lockObj); // Noncompliant
            if (condition)
            {
                Monitor.Exit(lockObj);
            }
        }
    }
}
