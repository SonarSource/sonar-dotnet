namespace ITs.JsonParser;

internal static class ConsoleHelper
{
    public static void WriteLineColor(string value, ConsoleColor color)
    {
        var before = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ForegroundColor = before;
    }
}
