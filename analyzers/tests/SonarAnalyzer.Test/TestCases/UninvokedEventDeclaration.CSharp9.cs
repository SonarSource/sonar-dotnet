using System;

public record EventInvocations
{
    public event EventHandler MyEvent1;
    public event EventHandler MyEvent2;
    public event EventHandler MyEvent3;
    public event EventHandler MyEvent4;
    public event EventHandler MyEvent5;
    public event EventHandler MyEvent6;
    public event EventHandler MyEvent7; // Noncompliant

    public event Action<object, EventArgs> MyAction1;
    public event Action<object, EventArgs> MyAction2;
    public event Action<object, EventArgs> MyAction3;
    public event Action<object, EventArgs> MyAction4;
    public event Action<object, EventArgs> MyAction5;
    public event Action<object, EventArgs> MyAction6;
    public event Action<object, EventArgs> MyAction7;  // Noncompliant

    public void InvokeAll()
    {
        MyEvent1(this, EventArgs.Empty);
        MyEvent2.Invoke(this, EventArgs.Empty);
        MyEvent3.DynamicInvoke(this, EventArgs.Empty);
        MyEvent4.BeginInvoke(this, EventArgs.Empty, null, null);
        this.MyEvent5(this, EventArgs.Empty);
        MyEvent6?.Invoke(this, EventArgs.Empty);

        MyAction1(this, EventArgs.Empty);
        MyAction2.Invoke(this, EventArgs.Empty);
        MyAction3.DynamicInvoke(this, EventArgs.Empty);
        MyAction4.BeginInvoke(this, EventArgs.Empty, null, null);
        this.MyAction5(this, EventArgs.Empty);
        MyAction6?.Invoke(this, EventArgs.Empty);
    }
}

public record EventInvocationsPositionalDeclaration(string Value)
{
    public event EventHandler MyEvent1;
    public event EventHandler MyEvent2; // Noncompliant

    public event Action<object, EventArgs> MyAction1;
    public event Action<object, EventArgs> MyAction2; // Noncompliant

    public void InvokeAll()
    {
        MyEvent1(this, EventArgs.Empty);
        MyAction1(this, EventArgs.Empty);
    }
}
