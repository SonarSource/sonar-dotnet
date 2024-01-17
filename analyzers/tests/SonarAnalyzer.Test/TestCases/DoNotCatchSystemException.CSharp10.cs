using System;
using System.IO;

try { }
catch (Exception e) when (e is not FileNotFoundException { Message.Length: 5 }) // Noncompliant
{
    // do something
}

try { }
catch (Exception e) when (e is FileNotFoundException { Message.Length: 42 })
{
    // do something
}
