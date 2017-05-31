using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method()
        {
            GC.KeepAlive(this); // Noncompliant {{Remove this call to 'GC.KeepAlive' (after converting to SafeHandle to encapsulate the unmanaged resource).}}
//          ^^^^^^^^^^^^^^^^^^
        }

        Program()
        {
            GC.KeepAlive(this); // Noncompliant
            GC.SuppressFinalize(this);

            Action a = () => GC.KeepAlive(this); // Noncompliant
        }

        int Property
        {
            get
            {
                GC.KeepAlive(this); // Noncompliant
                return 0;
            }
        }

    }
}
