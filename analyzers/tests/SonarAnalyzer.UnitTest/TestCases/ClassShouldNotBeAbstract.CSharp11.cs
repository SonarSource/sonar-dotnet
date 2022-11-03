using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    file abstract class Empty // Noncompliant {{Convert this 'abstract' class to a concrete type with a protected constructor.}}
//                      ^^^^^
    {

    }

    file abstract class Animal //Noncompliant {{Convert this 'abstract' class to an interface.}}
    {
        protected abstract void move();
        protected abstract void feed();

    }

    file class SomeBaseClass { }

    file abstract class Animal2 : SomeBaseClass //Compliant
    {
        protected abstract void move();
        protected abstract void feed();

    }
}
