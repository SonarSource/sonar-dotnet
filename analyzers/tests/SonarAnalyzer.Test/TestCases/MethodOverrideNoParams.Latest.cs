using System.Collections.Generic;

abstract class Base
{
    public virtual void Method(params int[] numbers) { }
    public virtual void Method(string s, params int[] numbers) { }
    public abstract void Method(string s, string s1, params int[] numbers);
}
abstract class Derived : Base
{
    public override void Method(int[] numbers) { }              // Noncompliant {{'params' should not be removed from an override.}}
    public override void Method(string s, int[] numbers) { }    // Noncompliant
    public override void Method(string s, string s1, params int[] numbers) { }
}

public interface IMath
{
    static virtual void Method(params int[] numbers) { }
    static virtual void Method(string s, params int[] numbers) { }
    static abstract void Method(string s, string s1, params int[] numbers);
}

public class Foo : IMath
{
    public static void Method(int[] numbers) { } // Compliant, rule does not apply to interfaces
    public static void Method(string s, int[] numbers) { } // Compliant, rule does not apply to interfaces
    public static void Method(string s, string s1, int[] numbers) { } // Compliant, rule does not apply to interfaces
}

class Base2
{
    public virtual void Method(params List<int> numbers) { }
    public virtual void Method(string s, params List<int> numbers) { }
}

class Derived2 : Base2
{
    public override void Method(List<int> numbers) { }              // Noncompliant {{'params' should not be removed from an override.}}
    public override void Method(string s, List<int> numbers) { }    // Noncompliant
}
