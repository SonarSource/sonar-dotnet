using System;

record Record { } // Fixed

record OtherRecord : IContract { } // Fixed

record SubRecord : OtherRecord { } // Compliant

record RedunantInterfaceImpl : IContract { } // Fixed

record PositionalRecord(int SomeProperty) { } // Fixed

record OtherPositionalRecord(int SomeProperty) : IContract { } // Fixed

record SubPositionalRecord(int SomeProperty) : OtherPositionalRecord(SomeProperty) { } // Compliant

record RedunantInterfaceImplPositionalRecord(int SomeProperty) : IContract { } // Fixed

interface IContract : IBaseContract { }

interface IBaseContract { }
