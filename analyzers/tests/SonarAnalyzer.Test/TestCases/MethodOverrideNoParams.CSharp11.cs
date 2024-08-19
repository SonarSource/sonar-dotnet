public interface IMath
{
    static virtual void Method(params int[] numbers) { }
    static virtual void Method(string s, params int[] numbers) { }
    static abstract void Method(string s, string s1, params int[] numbers);
}

public class Foo : IMath
{
    public static void Method(int[] numbers) { } // Compliant, rule does not apply to interfaces
    public static void Method(string s, int[] numbers) { } // Compliant, rule does not apply to interfaces
    public static void Method(string s, string s1, int[] numbers) { } // Compliant, rule does not apply to interfaces
}
