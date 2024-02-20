namespace CSharpLatest.CSharp10
{
    internal class AssignmentAndDeclarationInSameDeconstruction
    {
        public void Example()
        {
            int a = 5;
            (a, int b) = (16, 23);
        }
    }
}
