using System;
using System.Threading;

namespace Monitor_Goto
{
    class Program
    {
        private object obj = new object();

        public void Method1()
        {
            Monitor.Enter(obj); // Compliant

            goto Release;

        Release:
            Monitor.Exit(obj);
        }

        public void Method2()
        {
            Monitor.Enter(obj); // FN

            goto DoNotRelease;

        Release:
            Monitor.Exit(obj);

        DoNotRelease:
            return;
        }
    }
}
