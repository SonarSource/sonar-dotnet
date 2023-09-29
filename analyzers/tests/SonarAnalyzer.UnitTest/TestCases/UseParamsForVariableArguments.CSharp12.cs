using System;

class BaseClass(__arglist)
{
    void Foo()
    {
        ArgIterator argumentIterator = new ArgIterator(__arglist); // Error CS0190
    }
}
