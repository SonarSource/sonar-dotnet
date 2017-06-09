using System;

namespace Tests.Diagnostics
{
    public delegate void EventHandler(object s); // Noncompliant {{Change the signature of that event handler to match the specified signature.}}
    public delegate int EventHandler2(object s, EventArgs e); // Noncompliant
    public delegate void EventHandler3(int sender, EventArgs e); // Noncompliant
    public delegate void EventHandler4(object sender, int e); // Noncompliant
    public delegate void EventHandler5(object sender, EventArgs args); // Noncompliant

    public delegate void CorrectEventHandler(object sender, EventArgs e);
}
