using System;

record Record : Object { } // Noncompliant

record OtherRecord : Object, IContract { } // Noncompliant

record SubRecord : OtherRecord { } // Compliant

record RedunantInterfaceImpl : IContract, IBaseContract { } // Noncompliant

interface IContract : IBaseContract { }

interface IBaseContract { }
