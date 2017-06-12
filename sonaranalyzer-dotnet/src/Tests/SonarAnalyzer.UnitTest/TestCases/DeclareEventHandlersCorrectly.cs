using System;

namespace Tests.Diagnostics
{
    public delegate void EventHandler1(object s);
    public delegate int EventHandler2(object s, EventArgs e);
    public delegate void EventHandler3(int sender, EventArgs e);
    public delegate void EventHandler4(object sender, int e);
    public delegate void EventHandler5(object sender, EventArgs args);

    public delegate void CorrectEventHandler(object sender, EventArgs e);


    public class Foo
    {
        public event EventHandler1 Event1; // Noncompliant {{Change the signature of that event handler to match the specified signature.}}
//                   ^^^^^^^^^^^^^
        public event EventHandler2 Event2; // Noncompliant
        public event EventHandler3 Event3; // Noncompliant
        public event EventHandler4 Event4; // Noncompliant
        public event EventHandler5 Event5; // Noncompliant

        public event EventHandler1 Event1AsProperty // Noncompliant {{Change the signature of that event handler to match the specified signature.}}
//                   ^^^^^^^^^^^^^
        {
            add { }
            remove { }
        }

        public event CorrectEventHandler CorrectEvent;
    }
}