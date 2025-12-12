namespace CSharp8
{
    public partial interface IStaticFieldsOrder
    {
        public static int W = 2;

        public const int Const2 = 5;
    }
}

namespace CSharp13
{
    public partial class MyClass
    {
        public static partial int X => Y; // Compliant
        public static partial int Y => 42;

        public static int B = X;
    }
}
