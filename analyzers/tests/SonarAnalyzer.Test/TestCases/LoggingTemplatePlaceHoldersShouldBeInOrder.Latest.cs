using System;
using Microsoft.Extensions.Logging;

public union IntOrText(int, string);

public class Holder;

public static class Extensions
{
    extension(Holder source)
    {
        public int this[Index index] => 0;
    }
}

public class Program
{
    public void Method(ILogger logger, IntOrText first, IntOrText second, Holder source)
    {
        logger.LogInformation("{First} {Second}", first, second);   // Compliant
        logger.LogInformation("{First} {Second}", second, first);   // Noncompliant
                                                                    // Secondary @-1
        logger.LogInformation("{First} {Second}", source[^1], source[^2]);  // Compliant, element access is never matched by name
    }
    public void CollectionExpression(ILogger logger, int first, int second)
    {
        logger.LogInformation("{First} {Second}", [second, first]); // FN NET-4953
    }
}
