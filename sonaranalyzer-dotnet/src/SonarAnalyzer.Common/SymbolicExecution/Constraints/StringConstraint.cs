
namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public sealed class StringConstraint : SymbolicValueConstraint
    {
        public static readonly StringConstraint EmptyString = new StringConstraint();
        public static readonly StringConstraint FullOrNullString = new StringConstraint();
        public static readonly StringConstraint FullString = new StringConstraint();

        public override SymbolicValueConstraint OppositeForLogicalNot =>
            this == EmptyString ? FullOrNullString : null;
        

        public override string ToString()
        {
            return this == EmptyString? "EmptyString":
                   this == FullString ? "FullString" :
                   this == FullOrNullString ? "FullOrNullString" :
                   "null";
        }
    }
}
