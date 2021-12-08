namespace Net6
{
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
}
