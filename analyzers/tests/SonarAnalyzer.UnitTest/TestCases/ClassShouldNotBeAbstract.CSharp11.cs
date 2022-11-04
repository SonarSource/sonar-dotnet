using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    file abstract class Empty // Noncompliant {{Convert this 'abstract' class to a concrete type with a protected constructor.}}
//                      ^^^^^
    {

    }

    file abstract class Animal2 //Compliant
    {
        protected abstract void Move();
        string Foo() => "FOO";
    }
}
