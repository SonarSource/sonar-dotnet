using System;
using System.IO;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class UnusedLocalVariable2
    {
        void F1()
        {
            var packageA = PackageUtility.CreatePackage("Foo", "1.0");
            var packageB = PackageUtility.CreatePackage("Qux", "1.0");

            var localRepository = new MockPackageRepository { packageA, packageB }; // Noncompliant

            using (var x = new StreamReader("")) // Noncompliant
            {
                var v = 5; // Noncompliant
            }

            int a; // Noncompliant
            var b = (Action<int>)(
                _ =>
                {
                    int i; // Noncompliant
                    int j = 42;
                    Console.WriteLine("Hello, world!" + j);
                });

            b(5);

            string c;
            c = "Hello, world!";
            Console.WriteLine(c);

            var d = "";
            var e = new List<String> { d };
            Console.WriteLine(e);
        }

        void F2(int a)
        {
        }
    }
}
