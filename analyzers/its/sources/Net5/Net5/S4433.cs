using System.DirectoryServices;

namespace Net5
{
    public class S4433
    {
        private DirectoryEntry field = new ("path", "user", "pass", AuthenticationTypes.None);
    }
}
