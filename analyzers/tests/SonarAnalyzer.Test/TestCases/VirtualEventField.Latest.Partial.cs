using System;

public partial class PartialEvents
{
    public partial event EventHandler Compliant { add { } remove { } }
    public partial virtual event EventHandler BadEvent { add { } remove { } }   // Error [CS1585]
                                                                                    // Error@-1 [CS0102]
                                                                                    // Error@-2 [CS0621]
}
