using Microsoft.Extensions.Logging;

public class Program
{
    ILogger _logger;                        // Compliant with the default format

    // The message reports the default format, because that is what the members were matched against
    ILogger mylog { get; set; }             // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    //      ^^^^^
}
