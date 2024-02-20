namespace CSharpLatest.CSharp11
{
    internal class RelaxedShiftOperator
    {
        public static RelaxedShiftOperator operator >>(RelaxedShiftOperator other, string x)
            => other;

        public void Method(RelaxedShiftOperator a, string b)
        {
            _ = a >> b;
        }
    }
}
