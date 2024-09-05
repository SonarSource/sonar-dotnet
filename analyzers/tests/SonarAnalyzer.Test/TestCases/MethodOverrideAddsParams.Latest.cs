using System.Collections.Generic;

abstract record Base
{
    public virtual void Method(int[] numbers) { }

    public virtual void Method(string s, params int[] numbers) { }

    public abstract void Method(string s, string s1, int[] numbers);

    public virtual void Method(string s, string s1, string s2, params int[] numbers) { }

    public virtual void Method2(List<int> numbers) { }
}

abstract record Derived : Base
{
    public override void Method(params int[] numbers) { }                       // Noncompliant {{'params' should be removed from this override.}}

    public override void Method(string s, params int[] numbers) { }

    public override void Method(string s, string s1, params int[] numbers) { }  // Noncompliant

    public override void Method(string s, string s1, string s2, int[] numbers) { }

    public override void Method2(params List<int> numbers) { }                  // Noncompliant
}
