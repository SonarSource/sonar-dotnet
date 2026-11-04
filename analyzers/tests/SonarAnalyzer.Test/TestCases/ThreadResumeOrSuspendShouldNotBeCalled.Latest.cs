using System.Diagnostics;
using System.Threading;
using Microsoft.Win32.SafeHandles;

class ProcessResume
{
    void Method(ProcessStartInfo startInfo)
    {
        startInfo.StartSuspended = true;
        using var handle = SafeProcessHandle.Start(startInfo);
        handle.Resume();                // Compliant

        Thread.CurrentThread.Resume();  // Noncompliant
    }
}
