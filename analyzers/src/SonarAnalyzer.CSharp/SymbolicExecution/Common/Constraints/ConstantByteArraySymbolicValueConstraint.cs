namespace SonarAnalyzer.SymbolicExecution.Common.Constraints
{
    public class ConstantByteArraySymbolicValueConstraint : SymbolicValueConstraint
    {
        internal static readonly ConstantByteArraySymbolicValueConstraint Constant = new ConstantByteArraySymbolicValueConstraint();
        internal static readonly ConstantByteArraySymbolicValueConstraint Modified = new ConstantByteArraySymbolicValueConstraint();

        private ConstantByteArraySymbolicValueConstraint() { }

        public override SymbolicValueConstraint OppositeForLogicalNot =>
            this == Modified
                ? Constant
                : Modified;

        public override string ToString() =>
            this == Modified
                ? "Modified"
                : "Constant";
    }
}
