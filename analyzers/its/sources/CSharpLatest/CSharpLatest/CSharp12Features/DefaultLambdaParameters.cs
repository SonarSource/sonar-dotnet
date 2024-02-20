namespace CSharpLatest.CSharp12Features;

internal class DefaultLambdaParameters
{
    void Example ()
    {
        var incrementValue = (int source, int increment = 3) => source + increment;

        Console.WriteLine(incrementValue(5)); // 8
        Console.WriteLine(incrementValue(5, 2)); // 7
    }
}
