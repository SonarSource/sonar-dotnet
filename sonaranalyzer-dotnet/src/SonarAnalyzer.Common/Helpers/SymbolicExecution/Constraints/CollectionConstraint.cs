namespace SonarAnalyzer.Helpers.FlowAnalysis.Common
{
    public class CollectionConstraint : SymbolicValueConstraint
    {
        public static readonly CollectionConstraint Empty = new CollectionConstraint();
        public static readonly CollectionConstraint NotEmpty = new CollectionConstraint();

        public override SymbolicValueConstraint OppositeForLogicalNot =>
            this == Empty ? NotEmpty : Empty;

        public override string ToString()
        {
            return this == Empty
                ? "Empty"
                : "NotEmpty";
        }
    }
}
