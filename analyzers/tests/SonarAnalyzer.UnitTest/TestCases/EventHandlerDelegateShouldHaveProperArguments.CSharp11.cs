using System;

namespace Tests.Diagnostics
{
    interface IMyInterface
    {
        static virtual event EventHandler VirtualEvent;

        static virtual void VirtualMethod<T>(object sender, EventArgs e) where T : IMyInterface
        {
            T.VirtualEvent.Invoke(null, null);      // Noncompliant
            VirtualEvent.Invoke(null, null);        // Noncompliant
            T.VirtualEvent.Invoke(null, e);         // Compliant
            VirtualEvent.Invoke(null, e);           // Compliant
        }
    }
}
