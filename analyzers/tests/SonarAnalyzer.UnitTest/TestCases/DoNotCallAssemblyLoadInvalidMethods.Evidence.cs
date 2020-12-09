namespace Tests.Diagnostics
{
    using System.Configuration.Assemblies;
    using System.Reflection;
    using System.Security.Policy;
    using static System.Reflection.Assembly;

    public class Program
    {
        void Load()
        {
            Assembly.LoadFrom("a.dll", new Evidence()); // Noncompliant {{Replace this call to 'Assembly.LoadFrom' with 'Assembly.Load'.}}
//                   ^^^^^^^^
            Assembly.LoadFrom("a.dll", new Evidence(), new byte[] { }, AssemblyHashAlgorithm.MD5); // Noncompliant
            Assembly.LoadFile(@"c:\foo\a.dll", new Evidence()); // Noncompliant
            Assembly.LoadWithPartialName("a.dll", new Evidence()); // Noncompliant
        }
    }
}
