using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Tests.Diagnostics
{
    public class MyAttribute : Attribute
    {
        public bool isOneWay { get; set; }
    }

    [ServiceContract]
    interface IMyService1
    {
        [OperationContract]
        int MyServiceMethod();

        [OperationContract(IsOneWay = true)]
        int MyServiceMethod2(); // Noncompliant {{This method can't return any values because it is marked as one-way operation.}}
//      ^^^

        [OperationContract(IsOneWay = false)]
        int MyServiceMethod3();

        [OperationContract(IsTerminating = true)]
        [My(IsOneWay = true)]
        int MyServiceMethod4();

        [OperationContract(IsOneWay = "mistake")]
        int MyServiceMethod5();

        [OperationContract(IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginMyServiceMethod6();
    }
}
