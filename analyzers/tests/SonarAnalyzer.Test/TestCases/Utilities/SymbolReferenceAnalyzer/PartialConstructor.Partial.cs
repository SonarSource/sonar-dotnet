public partial class PartialConstructor
{
    public partial PartialConstructor()
    {
        this.A = 42;
    }

    public void Build()
    {
        var x = new PartialConstructor();
        x.A = 42;
    }
}
