using System;

public record VirtualEventField
{
    public event EventHandler OnRefueled; // Fixed

    public virtual event EventHandler Foo
    {
        add { Console.WriteLine("Base Foo.add called"); }
        remove { Console.WriteLine("Base Foo.remove called"); }
    }
}

public interface IVirtualEvent
{
    public static event EventHandler OnRefueled; // Fixed
}

public partial class PartialEvents
{
    public partial event EventHandler Compliant; // Compliant, a partial event cannot be virtual
    public partial event EventHandler BadEvent; // Fixed
                                                            // Error@-1 [CS0621]
                                                            // Error@-2 [CS1585]
}
