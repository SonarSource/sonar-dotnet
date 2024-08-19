using System;
using System.Collections.Generic;

file abstract class Empty
{
}

file abstract class OnlyAbstract    // Noncompliant {{Convert this 'abstract' class to an interface.}}
//                  ^^^^^^^^^^^^
{
    public abstract void Move();
}

file abstract class Animal2 //Compliant
{
    public abstract void Move();
    string Foo() => "FOO";
}
