namespace CSharpLatest.CSharp9Features;

public class S3060
{
    public int MyProperty { get; set; }

    public void Foo()
    {
        if (this is S3060 and { MyProperty: 2 }) { }
    }
}
