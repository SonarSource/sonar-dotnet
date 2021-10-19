using System;

record Record { } // Fixed

record struct RedunantInterfaceImpl : IContract, IBaseContract { } // FN

record struct RedunantInterfaceImplPositionalRecord(int SomeProperty) : IContract, IBaseContract { } // FN

interface IContract : IBaseContract { }

interface IBaseContract { }
