using System;

partial record R
{
    public event EventHandler SomeEvent;
    public static event EventHandler SomeStaticEvent;

    protected partial void OnFoo(EventArgs e);
}

partial class PartialEvents
{
    partial event EventHandler PartialEvent;
    static partial event EventHandler PartialStaticEvent;
}
