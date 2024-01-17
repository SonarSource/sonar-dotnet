using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class AnonymousDelegateEventUnsubscribe
    {
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public delegate void ChangedEventHandler2(object sender);
        public event ChangedEventHandler Changed;
        public event ChangedEventHandler2 Changed2;

        void Test_LambdaDiscard_StaticLambda()
        {
            Changed += (_, _) => { };
            Changed -= (_, _) => { }; //Noncompliant
            Changed -= (_, _) => Console.WriteLine(); // Noncompliant
            Changed -= static (_, _) => Console.WriteLine("x"); // Noncompliant
        }

        void Test_LambdaDiscard_StaticLambda_Compliant()
        {
            ChangedEventHandler x = (_, _) => { };
            Changed += x;
            Changed -= x;

            ChangedEventHandler2 y = static (sender) => { };
            Changed2 += y;
            Changed2 -= y;
        }
    }
}
