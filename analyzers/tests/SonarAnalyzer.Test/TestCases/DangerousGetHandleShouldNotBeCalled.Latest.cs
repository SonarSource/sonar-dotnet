using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public class ProcessHandles
{
    void SafeProcessHandles(ProcessStartInfo startInfo)
    {
        using SafeProcessHandle started = SafeProcessHandle.Start(startInfo);
        IntPtr fromStart = started.DangerousGetHandle();                                    // Noncompliant {{Refactor the code to remove this use of 'SafeHandle.DangerousGetHandle'.}}
        IntPtr fromOpen = SafeProcessHandle.Open(42).DangerousGetHandle();                  // Noncompliant
        if (SafeProcessHandle.TryOpen(42, out var opened))
        {
            IntPtr fromTryOpen = opened?.DangerousGetHandle() ?? IntPtr.Zero;               // Noncompliant
        }
    }

    void ProcessStartInfoHandles(ProcessStartInfo startInfo)
    {
        IntPtr input = startInfo.StandardInputHandle.DangerousGetHandle();                  // Noncompliant
        IntPtr output = startInfo.StandardOutputHandle?.DangerousGetHandle() ?? IntPtr.Zero; // Noncompliant
        foreach (SafeHandle inherited in startInfo.InheritedHandles)
        {
            IntPtr handle = inherited.DangerousGetHandle();                                 // Noncompliant
        }
    }

    void Compliant(SafeProcessHandle handle)
    {
        int processId = handle.ProcessId;
        handle.Kill();
    }
}
