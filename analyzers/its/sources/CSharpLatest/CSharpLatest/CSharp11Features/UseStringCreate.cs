namespace CSharpLatest.CSharp11
{
    public static class UseStringCreate
    {
        public static string Interpolate(string value) =>
            FormattableString.CurrentCulture($"Value: {value}");
    }
}
