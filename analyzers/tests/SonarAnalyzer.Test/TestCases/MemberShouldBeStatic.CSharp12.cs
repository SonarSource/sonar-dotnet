using System;

public class PrimaryClass(SomeService s1, SomeService s2, SomeService s3)
{
    public SomeService s1 = s1;
    public SomeService s2 { get; set; } = s2;

    private void Access_Parameter_OfPrimaryConstructor(int a, int b) // Compliant
    {
        _ = s1.Sum(a, b);
        _ = s2.Sum(a, b);
        _ = s3.Sum(a, b);
    }

    private void NoAccessTo_Parameter_OfPrimaryConstructor(int a, int b) // Noncompliant
    {
        _ = a + b;
    }

    private void MethodParameter_Hides_Parameter_OfPrimaryConstructor(int s1, int s2) // Noncompliant
    {
        _ = s1 + s2;
    }
}

public record PrimaryRecord(SomeService s1, SomeService s2, SomeService s3)
{
    public SomeService s1 = s1;
    public SomeService s2 { get; set; } = s2;

    private void Access_Parameter_OfPrimaryConstructor(int a, int b) // Compliant
    {
        _ = s1.Sum(a, b);
        _ = s2.Sum(a, b);
        _ = s3.Sum(a, b);
    }

    private void NoAccessTo_Parameter_OfPrimaryConstructor(int a, int b) // Noncompliant
    {
        _ = a + b;
    }

    private void MethodParameter_Hides_Parameter_OfPrimaryConstructor(int s1, int s2) // Noncompliant
    {
        _ = s1 + s2;
    }
}


public class SomeService
{
    private int x = 42;
    public int Sum(int a, int b) => a + b + x;
}
