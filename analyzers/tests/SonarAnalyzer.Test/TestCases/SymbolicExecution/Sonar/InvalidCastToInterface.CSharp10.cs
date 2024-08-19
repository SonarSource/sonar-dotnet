public interface IMyInterface { }

public class SomeImpl : IMyInterface { }

public class MyClass { }

public struct S
{
    public void M()
    {
        var myclass = new MyClass();
        MyClass a;
        (a, var b) = (myclass, (IMyInterface)myclass); // Noncompliant
    }
}
