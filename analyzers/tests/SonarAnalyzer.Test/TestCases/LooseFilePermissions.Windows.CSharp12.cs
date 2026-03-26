using System.Security.AccessControl;

class Program(FileSecurity fileSecurity)
{
    const string Everyone = "Everyone";

    void UpdatePermissionsUsingAccessControl()
    {
        fileSecurity.RemoveAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.ListDirectory, AccessControlType.Allow));
        fileSecurity.SetAccessRule(new FileSystemAccessRule(Everyone, FileSystemRights.Write, AccessControlType.Allow)); // Noncompliant
    }
}
