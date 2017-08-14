namespace SonarAnalyzer.Helpers.FlowAnalysis.Common
{
    public sealed class CollectionCapacityConstraint : SymbolicValueConstraint
    {
        public static readonly CollectionCapacityConstraint Empty = new CollectionCapacityConstraint();
        public static readonly CollectionCapacityConstraint NotEmpty = new CollectionCapacityConstraint();

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
