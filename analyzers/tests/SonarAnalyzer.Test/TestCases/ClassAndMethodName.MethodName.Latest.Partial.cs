namespace CSharp14
{
    public partial class PartialEvents
    {
        public partial event System.EventHandler NonCCCOmpliant { add { } remove { } }   // Compliant
        public partial event System.EventHandler ThisIsCompliant { add { } remove { } }  // Compliant
    }
}
