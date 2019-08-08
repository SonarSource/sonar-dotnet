
namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public sealed class StringConstraint : SymbolicValueConstraint
    {
        public static readonly StringConstraint EmptyString = new StringConstraint();
        public static readonly StringConstraint FullString = new StringConstraint();
        public static readonly StringConstraint FullOrNullString = new StringConstraint();
        // Currently FullOrNullString is never set as a constraint. It is there to imply the opposite of EmptyString
        public override SymbolicValueConstraint OppositeForLogicalNot =>
            this == EmptyString ? FullOrNullString : null;
        

        public override string ToString()
        {
            if (this == EmptyString)
            {
                return "EmptyString";
            }

            if (this == FullString)
            {
                return "FullString";
            }

            if (this == FullOrNullString)
            {
                return "FullOrNullString";
            }

            return "null";
        }
    }
}
