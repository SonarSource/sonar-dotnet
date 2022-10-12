namespace Net7.features
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
