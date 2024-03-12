using Microsoft.Extensions.Logging;

public class Compliant
{
    static readonly ILogger logger1;                      // Compliant

    private static readonly ILogger<int> logger2;         // Compliant

    private static readonly ILogger logger3, logger4;     // Compliant
}

public class Noncompliant
{
    ILogger logger1, logger2, logger3;
    //      ^^^^^^^ {{Make the logger 'logger1' private static readonly.}}
    //               ^^^^^^^ @-1 {{Make the logger 'logger2' private static readonly.}}
    //                        ^^^^^^^ @-2 {{Make the logger 'logger3' private static readonly.}}

    private ILogger logger4;                              // Noncompliant
    //              ^^^^^^^
    static ILogger<int> logger5;                          // Noncompliant
    readonly ILogger logger6;                             // Noncompliant

    private static ILogger logger7;                       // Noncompliant
    private readonly ILogger logger8;                     // Noncompliant

    public static readonly ILogger<object> logger10;      // Noncompliant
    protected internal static readonly ILogger logger12;  // Noncompliant
}
