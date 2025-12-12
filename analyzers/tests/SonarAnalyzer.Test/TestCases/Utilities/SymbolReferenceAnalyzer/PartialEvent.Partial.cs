using System;

public partial class Sample
{
    public partial event EventHandler Changed { add { changed += value; } remove { changed -= value; } }

    public void Go()
    {
        changed?.Invoke(null, null);
        Changed += Sample_Changed;
    }
}
