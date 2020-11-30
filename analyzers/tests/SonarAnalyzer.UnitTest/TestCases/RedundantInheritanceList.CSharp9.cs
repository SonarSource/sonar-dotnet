using System;

record Record : Object { } // Compliant - FN

record OtherRecord : Object, IContract { } // Compliant - FN

record SubRecord : OtherRecord { } // Compliant

record RedunantInterfaceImpl : IContract, IBaseContract { } // Compliant - FN

interface IContract : IBaseContract { }

interface IBaseContract { }
