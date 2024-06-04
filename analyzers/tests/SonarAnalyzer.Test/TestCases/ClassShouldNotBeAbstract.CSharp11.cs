using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    file abstract class Empty
    {
    }

    file abstract class OnlyAbstract    // Noncompliant {{Convert this 'abstract' class to an interface.}}
    //                  ^^^^^^^^^^^^
    {
        protected abstract void Move();
    }

    file abstract class Animal2 //Compliant
    {
        protected abstract void Move();
        string Foo() => "FOO";
    }
}
