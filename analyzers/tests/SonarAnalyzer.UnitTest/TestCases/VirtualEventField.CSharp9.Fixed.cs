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
