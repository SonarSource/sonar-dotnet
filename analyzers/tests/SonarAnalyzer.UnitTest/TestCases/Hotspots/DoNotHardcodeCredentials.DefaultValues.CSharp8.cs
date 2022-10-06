class Program
{
    private static string SomeMethod() => null;

    public void Concatenations()
    {
        var secret = SomeMethod();

        // Reassigned
        secret ??= "hardcoded";
        var a = "Server = localhost; Database = Test; User = SA; Password = " + secret;         // Compliant, this is not symbolic execution rule and ConstantValueFinder cannot detect that.
    }
}
