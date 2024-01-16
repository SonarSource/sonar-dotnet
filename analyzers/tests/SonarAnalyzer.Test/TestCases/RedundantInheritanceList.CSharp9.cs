using System;

record Record : Object { } // Noncompliant

record OtherRecord : Object, IContract { } // Noncompliant

record SubRecord : OtherRecord { } // Compliant

record RedunantInterfaceImpl : IContract, IBaseContract { } // Noncompliant

record PositionalRecord(int SomeProperty) : Object { } // Noncompliant

record OtherPositionalRecord(int SomeProperty) : Object, IContract { } // Noncompliant

record SubPositionalRecord(int SomeProperty) : OtherPositionalRecord(SomeProperty) { } // Compliant

record RedunantInterfaceImplPositionalRecord(int SomeProperty) : IContract, IBaseContract { } // Noncompliant

interface IContract : IBaseContract { }

interface IBaseContract { }
