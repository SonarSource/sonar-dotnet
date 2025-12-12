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

public interface IVirtualEvent
{
    public static virtual event EventHandler OnRefueled; // Noncompliant {{Remove this 'virtual' modifier of 'OnRefueled'.}}
}

public partial class PartialEvents
{
    public partial event EventHandler Compliant; // Compliant, a partial event cannot be virtual
    public partial virtual event EventHandler BadEvent; // Noncompliant
                                                            // Error@-1 [CS0621]
                                                            // Error@-2 [CS1585]
}
