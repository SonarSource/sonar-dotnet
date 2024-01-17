var topLevel = true;

void LocalFunctionInTopLevelStatement(int a, int b, int c, int d, int e) // Noncompliant
{
}

public record R(int a, int b, int c, int d, int e); // Compliant, this ParameterList syntax defines fields, not parameters

public record BaseRecord
{
    public BaseRecord(int i1, int i2, int i3, int i4) // Noncompliant
    {
    }
}
public record Extend : BaseRecord
{
    public Extend(int i1, int i2, int i3, int i4, string s1)    // Compliant, adds only one new
        : base(i1, i2, i3, i4)
    {
    }

    public Extend(int i1, int i2, int i3, int i4, string s1, string s2, string s3, string s4)   // Noncompliant {{Constructor has 4 new parameters, which is greater than the 3 authorized.}}
        : base(i1, i2, i3, i4)
    {
    }

}
