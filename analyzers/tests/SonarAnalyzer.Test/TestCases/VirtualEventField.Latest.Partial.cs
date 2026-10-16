using System;

public partial class PartialEvents
{
    public partial event EventHandler Compliant { add { } remove { } }
    public partial virtual event EventHandler BadEvent { add { } remove { } }   // Error [CS0267]
}
