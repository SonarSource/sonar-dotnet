using System;

namespace MyLibrary
{
    public interface ISomeMethodsInterface
    {
        void Method();
    }

    public interface MyInterface // Noncompliant {{Remove this interface or add members to it.}}
//                   ^^^^^^^^^^^
    {
    }

    public interface MyInterface2 : ISomeMethodsInterface // Noncompliant
    {
    }

    public interface MyInterface3
    {
        void Foo();
    }

    public interface MyInterface4
    {
        bool Bar { get; }
    }
}