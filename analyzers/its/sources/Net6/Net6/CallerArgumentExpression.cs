namespace Net6
{
    internal class CallerArgumentExpression
    {
        // Implicit using of CallerArgumentExpression from System.Runtime.CompilerServices which is declared in `GlobalUsingDirectives.cs`
        public static void SomeMethod(object param, [CallerArgumentExpression("param")] string message = null) =>
            Console.WriteLine(message);
    }
}
