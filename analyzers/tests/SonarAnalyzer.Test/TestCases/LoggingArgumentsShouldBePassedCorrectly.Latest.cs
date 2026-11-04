using System;
using Microsoft.Extensions.Logging;

public union ExceptionOrText(Exception, string);

public class Holder;

public static class Extensions
{
    extension(Holder source)
    {
        public Exception this[Index index] => null;
    }
}

public class Program
{
    public void Method(ILogger logger, Exception e, ExceptionOrText union, Holder source)
    {
        logger.LogError("Message {Argument}", union);           // Compliant, the union is not an Exception
        logger.LogError("Message {Argument}", source[^1]);      // Noncompliant
                                                                // Secondary@-1
        logger.LogError("Expected exception.", e);              // Noncompliant
                                                                // Secondary@-1
        logger.LogError("Expected exception.", [e]);            // FN NET-4952
    }
}
