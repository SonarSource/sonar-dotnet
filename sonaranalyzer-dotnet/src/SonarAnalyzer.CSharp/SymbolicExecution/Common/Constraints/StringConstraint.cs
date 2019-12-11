
namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public sealed class StringConstraint : SymbolicValueConstraint
    {
        public static readonly StringConstraint EmptyString = new StringConstraint();
        public static readonly StringConstraint FullString = new StringConstraint();
        public static readonly StringConstraint FullOrNullString = new StringConstraint();

        public static readonly StringConstraint WhiteSpaceString = new StringConstraint();
        public static readonly StringConstraint NotWhiteSpaceString = new StringConstraint();
        public static readonly StringConstraint FullNotWhiteSpaceString = new StringConstraint();
        // Currently FullOrNullString and NotWhiteSpaceString  is never set as a constraint. It is there to imply the opposite of EmptyString
        public override SymbolicValueConstraint OppositeForLogicalNot
        {
            get
            {
                if (this == EmptyString)
                {
                    return FullOrNullString;
                }
                if (this == WhiteSpaceString)
                {
                    return NotWhiteSpaceString;
                }
                return null;
            }
        }

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

            if (this == WhiteSpaceString)
            {
                return "FullOrNullString";
            }

            if (this == NotWhiteSpaceString)
            {
                return "FullOrNullString";
            }

            if (this == FullNotWhiteSpaceString)
            {
                return "FullNotWhiteSpaceString";
            }

            return "null";
        }
    }
}
