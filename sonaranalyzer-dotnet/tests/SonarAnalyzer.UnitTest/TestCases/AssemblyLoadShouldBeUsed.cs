namespace Tests.Diagnostics
{
    using System.Configuration.Assemblies;
    using System.Reflection;
    using System.Security.Policy;
    using static System.Reflection.Assembly;

    public class Program
    {
        void AssemblyLoadFrom()
        {
            Assembly.LoadFrom("a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadFrom' with 'Assembly.Load'.}}
//                   ^^^^^^^^
            Assembly.LoadFrom("a.dll", new Evidence()); // Noncompliant
            Assembly.LoadFrom("a.dll", new byte[] { }, AssemblyHashAlgorithm.MD5); // Noncompliant
            Assembly.LoadFrom("a.dll", new Evidence(), new byte[] { }, AssemblyHashAlgorithm.MD5); // Noncompliant
            LoadFrom("a.dll"); // Noncompliant
        }

        void AssemblyLoadFile()
        {
            Assembly.LoadFile(@"c:\foo\a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadFile' with 'Assembly.Load'.}}
//                   ^^^^^^^^
            Assembly.LoadFile(@"c:\foo\a.dll", new Evidence()); // Noncompliant
            LoadFile("a.dll"); // Noncompliant
        }

        void AssemblyLoadWithPartialName()
        {
            Assembly.LoadWithPartialName("a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadWithPartialName' with 'Assembly.Load'.}}
//                   ^^^^^^^^^^^^^^^^^^^
            Assembly.LoadWithPartialName("a.dll", new Evidence()); // Noncompliant
            LoadWithPartialName("a.dll"); // Noncompliant
        }
    }
}