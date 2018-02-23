using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Tests.Diagnostics
{
    public class AsyncVoidMethod
    {
        async void MyMethod() { } // Noncompliant {{Return 'Task' instead.}}
//            ^^^^
        async void MyMethod(object sender, EventArgs args) { } // Compliant


        // A lot of classes/interfaces in UWP do not inherit from EventArgs so we had to change the detection mechanism
        // See issue https://github.com/SonarSource/sonar-csharp/issues/704
        private interface ISuspendingEventArgs { }

        async void MyOtherMethod1(object o, ISuspendingEventArgs args) { } // Compliant
        private async void OnSuspending(object sender, ISuspendingEventArgs e) { } // Compliant - ends with EventArgs and 1st param is object sender
    }
}
