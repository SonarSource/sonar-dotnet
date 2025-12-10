// https://sonarsource.atlassian.net/browse/NET-429
namespace CSharp13
{
    public partial class PartialProperties
    {
        private int x;
        private int y;

        public partial int X { get; set; }

        public partial int Y
        {
            get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
            set { x = value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
        }
    }
}
