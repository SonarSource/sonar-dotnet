using System;

public class BaseClass(__arglist) // Nomcompliant {{Use the 'params' keyword instead of '__arglist'.}}
//           ^^^^^^^^^
{
    void Foo()
    {
        ArgIterator argumentIterator = new ArgIterator(__arglist); // Error [CS0190]
    }
}
