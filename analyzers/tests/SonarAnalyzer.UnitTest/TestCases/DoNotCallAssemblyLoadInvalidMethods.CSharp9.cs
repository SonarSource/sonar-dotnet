using System.Configuration.Assemblies;
using System.Reflection;
using System.Security.Policy;
using static System.Reflection.Assembly;

Assembly.LoadFrom("a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadFrom' with 'Assembly.Load'.}}
Assembly.LoadFrom("a.dll", new byte[] { }, AssemblyHashAlgorithm.MD5); // Noncompliant
LoadFrom("a.dll"); // Noncompliant
Assembly.LoadFile(@"c:\foo\a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadFile' with 'Assembly.Load'.}}
LoadFile("a.dll"); // Noncompliant
Assembly.LoadWithPartialName("a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadWithPartialName' with 'Assembly.Load'.}}
LoadWithPartialName("a.dll"); // Noncompliant

Assembly.Load("a.dll"); // Compliant

record R
{
    public void Foo()
    {
        Assembly.LoadFrom("a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadFrom' with 'Assembly.Load'.}}
        Assembly.Load("a.dll"); // Compliant
    }
}
