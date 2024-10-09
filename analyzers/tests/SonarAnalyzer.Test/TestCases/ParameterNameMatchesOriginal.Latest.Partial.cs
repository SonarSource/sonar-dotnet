namespace CSharp13
{
    public partial class PartialIndexers
    {
        // https://sonarsource.atlassian.net/browse/NET-423
        public partial int this[int index] => 42; // FN
    }
}
