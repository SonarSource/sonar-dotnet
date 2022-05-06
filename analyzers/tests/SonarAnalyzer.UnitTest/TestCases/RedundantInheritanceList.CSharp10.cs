using System;

interface IBaseContract { }

interface IContract : IBaseContract { }

record Record : Object { } // Noncompliant

record struct RedunantInterfaceImpl : IContract, IBaseContract { } // Noncompliant

record struct RedunantInterfaceImplPositionalRecord(int SomeProperty) : IContract, IBaseContract { } // Noncompliant
