using System;

interface IBaseContract { }

interface IContract : IBaseContract { }

record Record { } // Fixed

record struct RedunantInterfaceImpl : IContract { } // Fixed

record struct RedunantInterfaceImplPositionalRecord(int SomeProperty) : IContract { } // Fixed
