namespace Net7.features
{
    public static class UseStringCreate
    {
        public static string Interpolate(string value) =>
            FormattableString.CurrentCulture($"Value: {value}");
    }
}
