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
            Assembly.LoadFrom("a.dll", new byte[] { }, AssemblyHashAlgorithm.MD5); // Noncompliant
            LoadFrom("a.dll"); // Noncompliant
        }

        void AssemblyLoadFile()
        {
            Assembly.LoadFile(@"c:\foo\a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadFile' with 'Assembly.Load'.}}
//                   ^^^^^^^^
            LoadFile("a.dll"); // Noncompliant
        }

        void AssemblyLoadWithPartialName()
        {
            Assembly.LoadWithPartialName("a.dll"); // Noncompliant {{Replace this call to 'Assembly.LoadWithPartialName' with 'Assembly.Load'.}}
//                   ^^^^^^^^^^^^^^^^^^^
            LoadWithPartialName("a.dll"); // Noncompliant
        }
    }
}

namespace Repro
{
    using System;
    using System.Reflection;

    // https://sonarsource.atlassian.net/browse/NET-2099
    public class NET2099
    {
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom("NonexistentDLL");
        }

        private static Assembly OnTypeResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom("NonexistentDLL"); // FN
        }

        private static Assembly OnResourceResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom("NonexistentDLL"); // FN
        }

        private static Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom("NonexistentDLL"); // FN
        }

        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += LocalOnAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
            {
                return Assembly.LoadFrom("NonexistentDLL");
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return Assembly.LoadFrom("NonexistentDLL");
            };

            AppDomain.CurrentDomain.TypeResolve += OnTypeResolve;
            AppDomain.CurrentDomain.ResourceResolve += OnResourceResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;

            Assembly LocalOnAssemblyResolve(object sender, ResolveEventArgs args)
            {
                return Assembly.LoadFrom("NonexistentDLL");
            }
        }
    }
}
