using System;
using System.Security.AccessControl;
using System.Security.Principal;

class Program(FileSecurity fileSecurity)
{
    private const string Everyone = "Everyone";

    public void UpdatePermissionsUsingAccessControl(DirectorySecurity directorySecurity)
    {
        fileSecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.ListDirectory, AccessControlType.Allow));
        fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
    }
}
