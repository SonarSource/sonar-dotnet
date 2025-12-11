using System;

public partial class PartialEvents
{
    public partial event EventHandler Compliant
    {
        add { compliant += value; }
        remove { compliant -= value; }
    }

    public partial event EventHandler NonCompliant
    {
        add { nonCompliant += value; }
        remove { nonCompliant -= value; }
    }
}
