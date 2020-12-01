using System;

public record VirtualEventField
{
    public virtual event EventHandler OnRefueled; // Noncompliant {{Remove this 'virtual' modifier of 'OnRefueled'.}}
//         ^^^^^^^

    public virtual event EventHandler Foo
    {
        add { Console.WriteLine("Base Foo.add called"); }
        remove { Console.WriteLine("Base Foo.remove called"); }
    }
}
