namespace CSharpLatest.CSharp10Features;

// In .NET 6, the constructors/methods with MutexSecurity and MutexRights are in the MutexAcl class inside the Microsoft.Windows.Compatibility NuGet.
// However, Microsoft.Windows.Compatibility does not contain DLLs, but rather a target to liaise with the .NET Framework DLLs.
// So we test them in the ITs.
// https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutexacl.tryopenexisting?view=dotnet-plat-ext-6.0
public class MutexAclTest
{
    public static void Noncompliant(System.Security.AccessControl.MutexSecurity mutexSecurity, bool cond)
    {
        var m0 = MutexAcl.Create(true, "foo", out var mutexWasCreated, mutexSecurity); // FN
        if (cond)
        {
            m0.ReleaseMutex();
        }
        m0 = MutexAcl.OpenExisting("foo", System.Security.AccessControl.MutexRights.FullControl); // FN
        m0.WaitOne();
        if (cond)
        {
            m0.ReleaseMutex();
        }
        if (MutexAcl.TryOpenExisting("foo", System.Security.AccessControl.MutexRights.FullControl, out var mutex)) // FN
        {
            mutex.WaitOne();
            if (cond)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    public static void CompliantAcquiredNotReleased(System.Security.AccessControl.MutexSecurity mutexSecurity)
    {
        var m0 = MutexAcl.Create(true, "foo", out var mutexWasCreated, mutexSecurity);
        var m1 = MutexAcl.OpenExisting("foo", System.Security.AccessControl.MutexRights.FullControl);
        m1.WaitOne();
    }

    public static void CompliantNoAcquire(System.Security.AccessControl.MutexSecurity mutexSecurity)
    {
        var m0 = MutexAcl.Create(false, "foo", out var mutexWasCreated, mutexSecurity);
        var m1 = MutexAcl.OpenExisting("foo", System.Security.AccessControl.MutexRights.FullControl);
    }
}
