using Microsoft.Extensions.Logging;

public class Program
{
    ILogger chocolate;                     // Compliant, matches the configured format

    // '_logger' is compliant with the default format, but not with the configured one
    ILogger _logger;                        // Noncompliant {{Rename this field '_logger' to match the regular expression '^chocolate$'.}}
    ILogger mylog { get; set; }             // Noncompliant {{Rename this property 'mylog' to match the regular expression '^chocolate$'.}}
    //      ^^^^^
}
