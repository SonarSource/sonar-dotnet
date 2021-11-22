namespace Tests.Diagnostics
{
    public partial class PartialMethods
    {
        partial void UnusedMethod(); // Noncompliant
    }
}
