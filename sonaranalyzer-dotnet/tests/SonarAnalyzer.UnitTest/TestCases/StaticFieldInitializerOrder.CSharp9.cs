using System;

public record StaticFieldsOrder
{
    public static string s1 = new string('x', Const); // Compliant - const
    public static string s2 = new string('x', Y); // Noncompliant

    public static int[] ArrY1 = new[] { Y }; // Noncompliant
    public static Action<int> A = (i) => { var x = i + StaticFieldsOrder.Y; }; // Okay??? Might or might not. For now we don't report on it
    public static int X = Y; // Noncompliant
    public static int X2 = M(StaticFieldsOrder.Y); // Noncompliant Y at this time is still assigned default(int), i.e. 0
    public static int Y = 42;
    public static int Z = Y; // Noncompliant - FP
    public static int U = Const; // Compliant
    public static int[] ArrY2 = new[] { Y }; // Noncompliant - FP
    public static int[] ArrC = new[] { Const };
    public const int Const = 5;

    public static int M(int i) { return i; }
}
