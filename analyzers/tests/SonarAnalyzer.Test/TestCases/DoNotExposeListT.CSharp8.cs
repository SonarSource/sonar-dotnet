using System.Collections.Generic;

public interface IMust
{
    public abstract List<string> InterfaceMethod();         // Noncompliant
}

public abstract class Base
{
    public abstract List<string> Method();                  // Noncompliant
    public abstract List<string> Property { get; }          // Noncompliant
}

public class Overriding : Base, IMust
{
    public override List<string> Method() => null;      // Compliant, can't change the return type
    public override List<string> Property => null;      // Compliant, can't change the return type

    List<string> IMust.InterfaceMethod() => null;       // Compliant, can't change the return type
}
