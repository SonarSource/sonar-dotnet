using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Tests.Diagnostics
{
    class Program
    {
        private const string Everyone = "Everyone";

        public void UpdatePermissionsUsingAccessControl(FileSecurity fileSecurity, DirectorySecurity directorySecurity)
        {
            fileSecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.ListDirectory, AccessControlType.Deny));
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.ListDirectory, AccessControlType.Deny));
            fileSecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.ListDirectory, AccessControlType.Allow));
            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant

            fileSecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
            fileSecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow)); // Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow)); // Noncompliant

            fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Read | FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Read | FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.FullControl, AccessControlType.Allow)); // Noncompliant
            fileSecurity.SetAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.FullControl, AccessControlType.Allow)); // Noncompliant

            fileSecurity.AddAccessRule(new FileSystemAccessRule(new NTAccount("Everyone"), FileSystemRights.FullControl, AccessControlType.Allow)); // Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow)); // Noncompliant
            fileSecurity.AddAccessRule(new FileSystemAccessRule(new NTAccount(Everyone), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow)); // Noncompliant

            fileSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.Modify, AccessControlType.Allow)); // Noncompliant

            directorySecurity.SetAccessRule(new FileSystemAccessRule("User", FileSystemRights.Write, AccessControlType.Allow));
            directorySecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.Write, AccessControlType.Allow));
            directorySecurity.AddAccessRule(new FileSystemAccessRule("User", FileSystemRights.Write, AccessControlType.Deny));
            directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Deny));
            directorySecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));
            directorySecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
            directorySecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
        }

        public void InVariable(FileSecurity fileSecurity)
        {
            var unsafeAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow);
//                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
//                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1
            fileSecurity.RemoveAccessRule(unsafeAccessRule);

            fileSecurity.AddAccessRule(unsafeAccessRule); // Noncompliant
            fileSecurity.SetAccessRule(unsafeAccessRule); // Noncompliant

            var safeAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny);
            fileSecurity.AddAccessRule(safeAccessRule);
            fileSecurity.SetAccessRule(safeAccessRule);

            var rule = new FileSystemAccessRule("User", FileSystemRights.Read, AccessControlType.Allow);
            rule = new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Allow);

            fileSecurity.AddAccessRule(rule); // Compliant - FN, we look only at the object creation and don't track state changes.
        }

        public void InsideTryCatch(FileSecurity fileSecurity)
        {
            var rule = new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Allow);
//                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            try
            {
                fileSecurity.AddAccessRule(rule); // Noncompliant
            }
            catch { }
        }

        private void InsideLocalFunction(FileSecurity fileSecurity)
        {
            var rule = new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Allow);
//                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            void LocalFunction()
            {
                fileSecurity.AddAccessRule(rule); // Noncompliant
            }
        }

        private void InsideLambda(FileSecurity fileSecurity)
        {
            Func<FileSystemAccessRule> ruleFactory = () => new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Allow);

            fileSecurity.AddAccessRule(ruleFactory()); // Compliant - FN, not supported
        }
    }
}
