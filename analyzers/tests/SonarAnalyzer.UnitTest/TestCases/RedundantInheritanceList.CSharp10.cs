using System;

record Record : Object { } // Noncompliant

record struct RedunantInterfaceImpl : IContract, IBaseContract { } // FN

record struct RedunantInterfaceImplPositionalRecord(int SomeProperty) : IContract, IBaseContract { } // FN

interface IContract : IBaseContract { }

interface IBaseContract { }
