using System;
using System.Security.AccessControl;
using System.Security.Principal;

FileSecurity fileSecurity = null;
DirectorySecurity directorySecurity = null;

fileSecurity.SetAccessRule(new ("User", FileSystemRights.ListDirectory, AccessControlType.Allow));
fileSecurity.AddAccessRule(new ("User", FileSystemRights.ListDirectory, AccessControlType.Allow));
fileSecurity.AddAccessRule(new ("User", FileSystemRights.ListDirectory, AccessControlType.Deny));
fileSecurity.AddAccessRule(new ("Everyone", FileSystemRights.ListDirectory, AccessControlType.Deny));
fileSecurity.RemoveAccessRule(new ("Everyone", FileSystemRights.ListDirectory, AccessControlType.Allow));
fileSecurity.SetAccessRule(new ("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // FN
fileSecurity.AddAccessRule(new ("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // FN

void InVariable(FileSecurity fileSecurity)
{
    FileSystemAccessRule unsafeAccessRule = new ("Everyone", FileSystemRights.FullControl, AccessControlType.Allow);


    fileSecurity.RemoveAccessRule(unsafeAccessRule);

    fileSecurity.AddAccessRule(unsafeAccessRule); // FN
    fileSecurity.SetAccessRule(unsafeAccessRule); // FN

    FileSystemAccessRule safeAccessRule = new ("Everyone", FileSystemRights.FullControl, AccessControlType.Deny);
    fileSecurity.AddAccessRule(safeAccessRule);
    fileSecurity.SetAccessRule(safeAccessRule);

    FileSystemAccessRule rule = new ("User", FileSystemRights.Read, AccessControlType.Allow);
    rule = new ("Everyone", FileSystemRights.Read, AccessControlType.Allow);

    fileSecurity.AddAccessRule(rule); // Compliant - FN, we look only at the object creation and don't track state changes.
}
