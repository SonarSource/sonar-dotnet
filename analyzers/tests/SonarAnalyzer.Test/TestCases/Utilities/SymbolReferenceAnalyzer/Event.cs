using System;

public class Sample
{
    public event EventHandler Changed;

    public void Go()
    {
        Changed?.Invoke(null, null);
        Changed += Sample_Changed;
    }

    private void Sample_Changed(object sender, EventArgs e) { }
}
