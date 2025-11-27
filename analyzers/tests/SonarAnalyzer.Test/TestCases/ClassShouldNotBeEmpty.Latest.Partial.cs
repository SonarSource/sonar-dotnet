using System;

public partial class PartialConstructor
{
    public partial PartialConstructor() { }
}

public partial class PartialEvent
{
    public partial event EventHandler MyEvent { add { } remove { }  }
}
