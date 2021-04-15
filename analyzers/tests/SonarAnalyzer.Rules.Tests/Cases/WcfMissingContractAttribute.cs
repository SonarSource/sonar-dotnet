using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    [System.ServiceModel.ServiceContract]
    interface IMyService1
    {
        [System.ServiceModel.OperationContract]
        int MyServiceMethod();
        int MyServiceMethod2();
    }

    [System.ServiceModel.ServiceContract]
    interface IMyService2 // Noncompliant {{Add the 'OperationContract' attribute to the methods of this interface.}}
//            ^^^^^^^^^^^
    {
        int MyServiceMethod();
    }

    class IMyService3 // Noncompliant {{Add the 'ServiceContract' attribute to  this class.}}
    {
        [System.ServiceModel.OperationContract]
        int MyServiceMethod() { return 1; }
    }
}
