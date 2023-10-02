using Person = (string, string);

public class Sample(string str)
{
    public void Method()
    {
        string sample1 = $"{nameof(str)}";    // Noncompliant, compile-time interpolated string 
        string sample2 = nameof(Person);      // Noncompliant, nameof of alias is compile-time
        string sample3 = $"{nameof(Person)}"; // Noncompliant
    }
}
