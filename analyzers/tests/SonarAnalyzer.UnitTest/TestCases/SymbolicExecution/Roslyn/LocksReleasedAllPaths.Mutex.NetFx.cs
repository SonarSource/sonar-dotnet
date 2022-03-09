using System;
using System.Security.AccessControl;
using System.Threading;

// .NET Framework 4.8 APIs https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex.-ctor?view=netframework-4.8

// In .NET 6, the constructors/methods with MutexSecurity and MutexRights are in the MutexAcl class inside the Microsoft.Windows.Compatibility NuGet.
// However, Microsoft.Windows.Compatibility does not contain DLLs, but rather a target to liaise with the .NET Framework DLLs.
// So we test them in the ITs.
// https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutexacl.tryopenexisting?view=dotnet-plat-ext-6.0

public class MutexTest
{
    public static void Noncompliant(MutexSecurity mutexSecurity, bool cond)
    {
        // Note that Dispose() closes the underlying WaitHandle, but does not release the mutex
        var m0 = new Mutex(true, "foo", out var mutexWasCreated, mutexSecurity); // Noncompliant
        if (cond)
        {
            m0.ReleaseMutex();
        }
        m0.Dispose();
    }

    public static void CompliantAcquiredNotReleased(MutexSecurity mutexSecurity)
    {
        var m0 = new Mutex(true, "foo", out var mutexWasCreated, mutexSecurity);
        m0.Dispose();
    }

    public static void CompliantNoAcquire(MutexSecurity mutexSecurity)
    {
        var m2 = new Mutex(false, "foo", out var mutexWasCreated, mutexSecurity);
    }

    // Code modified from the MS Docs samples. https://docs.microsoft.com/en-us/dotnet/standard/threading/mutexes
    public static void CompliantFromDocs(string mutexName, MutexSecurity mutexSecurity)
    {
        Mutex m = null;
        var doesNotExist = false;
        var unauthorized = false;

        // Attempt to open the named mutex.
        try
        {
            m = Mutex.OpenExisting(mutexName);
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            doesNotExist = true;
        }
        catch (UnauthorizedAccessException)
        {
            unauthorized = true;
        }

        // The value of this variable is set by the mutex constructor.
        // - 'true' if the named system mutex was created
        // - 'false' if the named mutex already existed.
        var mutexWasCreated = false;
        if (doesNotExist)
        {
            // The mutex does not exist, so create it.
            m = new Mutex(true, mutexName, out mutexWasCreated, mutexSecurity);
            if (!mutexWasCreated)
            {
                // unable to create the mutex
                return;
            }
        }
        else if (unauthorized)
        {
            try
            {
                m = Mutex.OpenExisting(mutexName, MutexRights.ReadPermissions | MutexRights.ChangePermissions);
                // Update the ACL. This requires MutexRights.ChangePermissions.
                m.SetAccessControl(mutexSecurity);
                // open again
                m = Mutex.OpenExisting(mutexName); // the mutex is not yet acquired
                if (Foo())
                {
                    return; // compliant
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }

        // If this program created the mutex, it already owns the mutex.
        if (!mutexWasCreated)
        {
            // Enter the mutex, and hold it until the program exits.
            try
            {
                m.WaitOne();
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Unauthorized access: {0}", ex.Message);
            }
        }

        m.ReleaseMutex();
        m.Dispose();
    }

    static bool Foo() => true;
}
