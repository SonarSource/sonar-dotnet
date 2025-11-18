using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

var fs0 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant
FileStream fs1 = new(@"c:\foo.txt", FileMode.Open);     // Noncompliant

var fs2 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant, passed to a method
NoOperation(fs2);

FileStream fs3; // Compliant - not instantiated
using var fs5 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant

using FileStream fs6 = new(@"c:\foo.txt", FileMode.Open); // Compliant

void NoOperation(object x) { }
