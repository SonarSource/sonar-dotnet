using System;
using System.IO;

try { }
catch (Exception e) { } // Noncompliant {{Catch a list of specific exception subtype or use exception filters instead.}}

try { }
catch (Exception e) when (e is FileNotFoundException or is IOException)
{
}

try { }
catch (Exception e) when (e is not FileNotFoundException) // FN
{
    // do something
}

record R
{
    string P
    {
        init
        {
            try { }
            catch (Exception e) { } // Noncompliant
        }
    }
}
