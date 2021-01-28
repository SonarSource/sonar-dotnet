public class Noncompliant
{
    private const int Zero = 0;

    public static void InitializedWithZeroSize()
    {
        var zero = new int[0]; // Noncompliant {{Declare this empty array using Array.Empty<int>().}}
        //         ^^^^^^^^^^
        var zeroZero = new int[00]; // Noncompliant
        var zeroConstant = new int[Zero];
    }
    public static void EmptyInitialization()
    {
        var empty = new int[] { }; // Noncompliant
        var emptyWithComment = new int[] { /* comment */ }; // Noncompliant
        var emptyDynamic = new dynamic[] { }; // Noncompliant {{Declare this empty array using Array.Empty<dynamic>().}}
    }
    public static void InMethodCall()
    {
        MethodCall(new int[0]); // Noncompliant
    }
    private static void MethodCall<T>(T[] array) { }
}
public class Compliant
{
    public static void Initialize()
    {
        var nonZeroSize = new int[42]; // Compliant
        var withArguments = new[] { 1 }; // Compliant
        var mulitDemensional = new int[0, 0]; // Compliant
    }
}
