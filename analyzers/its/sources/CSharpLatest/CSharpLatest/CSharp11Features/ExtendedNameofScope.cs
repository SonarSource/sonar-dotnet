namespace CSharpLatest.CSharp11Features;

internal class ExtendedNameofScope
{
    [Obsolete(nameof(message))]
    public static void LogMessage(string? message)
    {
        Console.WriteLine(message.Length);
    }

    [Obsolete(nameof(TParameter))]
    public static int DoWork<TParameter>(TParameter parameter) =>
        parameter.GetHashCode();
}
