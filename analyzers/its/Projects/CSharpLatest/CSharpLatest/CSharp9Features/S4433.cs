using System.DirectoryServices;

namespace CSharpLatest.CSharp9Features;

public class S4433
{
    private DirectoryEntry field = new ("path", "user", "pass", AuthenticationTypes.None);
}
