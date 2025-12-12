using System.IO;
using System.Threading.Tasks;
using System;

var ms = new MemoryStream();
ms.Dispose(); // Noncompliant {{Await DisposeAsync instead.}}
