using System.Security.AccessControl;

namespace Net5
{
    public class S2612
    {
        void Foo(FileSecurity fileSecurity)
        {
            FileSystemAccessRule unsafeAccessRule = new ("Everyone", FileSystemRights.FullControl, AccessControlType.Allow);
            fileSecurity.AddAccessRule(unsafeAccessRule);
            fileSecurity.SetAccessRule(unsafeAccessRule);
        }
    }
}
