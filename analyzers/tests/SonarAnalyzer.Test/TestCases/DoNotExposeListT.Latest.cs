using System;
using System.Collections.Generic;

var x = 1;

List<int> Foo() => new();

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

public class Bar<T>
{
    public List<T> Method1<T>(T arg) => null; // Noncompliant
}

public record R<T>
{
    public List<T> Method1<T>(T arg) => null; // Noncompliant {{Refactor this method to use a generic collection designed for inheritance.}}

    private List<T> Method2<T>(T arg) => null;

    private object Method3(int arg) => null;

    public List<int> Method4(List<string> someParam) => null;
//         ^^^^^^^^^ Noncompliant
//                           ^^^^^^^^^^^^ Noncompliant@-1

    public List<T> field, field2;
//         ^^^^^^^ Noncompliant {{Refactor this field to use a generic collection designed for inheritance.}}

    public List<int> Property { get; init; }
//         ^^^^^^^^^ Noncompliant {{Refactor this property to use a generic collection designed for inheritance.}}

    public R(List<R<int>> bars) { } // Noncompliant  {{Refactor this constructor to use a generic collection designed for inheritance.}}

    public void Foo()
    {
        Action<List<int>> x = static x => { };
    }
}

public record R2(List<int> Property);  // FN #6416

public partial class PartialProperties
{
    public partial List<int> Result { get; set; } // Noncompliant
}

public partial class PartialProperties
{
    public partial List<int> Result { get => null; set { } } // Noncompliant
}
