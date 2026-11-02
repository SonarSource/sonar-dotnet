public record OverrideOnlyGetHashCode // Compliant - `Equals can't be overridden for records`
{
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

namespace CSharp15
{
    public struct StructOverridesOnlyEquals // Noncompliant
    {
        public override bool Equals(object obj) => true;
    }

    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union
    public union UnionRepro(UnionCaseA, UnionCaseB)
    {
        public override bool Equals(object obj) => true; // FN NET-4954
    }

    public union UnionBothOverridden(UnionCaseA, UnionCaseB)
    {
        public override bool Equals(object obj) => true; // Compliant, GetHashCode is also overridden
        public override int GetHashCode() => 0;
    }

    public record class UnionCaseA(string Name);
    public record class UnionCaseB(string Name);
}
