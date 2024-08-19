namespace CSharpLatest.CSharp11Features;

public static class UseStringCreate
{
    public static string Interpolate(string value) =>
        FormattableString.CurrentCulture($"Value: {value}");
}
