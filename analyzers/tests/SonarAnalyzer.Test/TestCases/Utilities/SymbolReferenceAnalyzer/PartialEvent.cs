using System;

public partial class Sample
{
    private EventHandler changed;
    public partial event EventHandler Changed;
    private void Sample_Changed(object sender, EventArgs e) { }
}
