namespace CSharp13
{
    public partial class PartialStaticProperty //Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        public static partial string Prop { get => "fourty-two"; set { } }
    }
}

namespace CSharp14
{
    public partial class StaticPartialConstructor // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        static partial StaticPartialConstructor(); // Error [CS0267]
        // Error@-1 [CS0111]
        public static string Prop { get; set; }
    }

    public partial class StaticPartialEvent // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        public static partial event System.EventHandler<System.EventArgs> PartialEvent { add { } remove { } }
    }
    public partial class PartialConstructor
    {
        partial PartialConstructor() { }
    }

    public partial class InstancePartialEvent
    {
        public partial event System.EventHandler<System.EventArgs> PartialEvent { add { } remove { } }
    }
}
