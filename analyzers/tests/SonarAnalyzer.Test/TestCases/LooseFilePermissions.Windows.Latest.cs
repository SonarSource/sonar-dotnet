using System.Security.AccessControl;

public class Cat;
public class Dog;

public union Pet(Cat, Dog)
{
    static void Loosen(FileSecurity fileSecurity)
    {
        fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));      // Noncompliant
        fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Deny));       // Compliant
    }
}

public closed class ClosedBase
{
    protected void Loosen(FileSecurity fileSecurity) =>
        fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));      // Noncompliant
}

public static class Extensions
{
    extension(FileSecurity fileSecurity)
    {
        public void Loosen() =>
            fileSecurity.SetAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));  // Noncompliant
    }
}
