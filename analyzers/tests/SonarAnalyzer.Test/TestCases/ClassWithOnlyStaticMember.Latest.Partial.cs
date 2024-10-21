namespace CSharp13
{
    public partial class PartialStaticProperty //Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        public static partial string Prop { get => "fourty-two"; set { } }
    }
}
