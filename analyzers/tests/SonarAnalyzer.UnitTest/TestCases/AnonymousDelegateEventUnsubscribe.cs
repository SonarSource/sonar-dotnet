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

        void Test()
        {
            Changed += (obj, args) => { };
            Changed -= (obj, args) => { }; //Noncompliant {{Unsubscribe with the same delegate that was used for the subscription.}}
//                  ^^^^^^^^^^^^^^^^^^^^^

            Changed -= (obj, args) => Console.WriteLine(); // Noncompliant - single statement
//                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Changed -= delegate (object sender, EventArgs e) { }; // Noncompliant
            Changed2 -= delegate { }; // Noncompliant

            ChangedEventHandler x = (obj, args) => { };
            Changed -= x;
        }
    }
}
