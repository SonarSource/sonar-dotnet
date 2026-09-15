using Serilog;

public class Program
{
    string mylogger;                        // Compliant
    ILogger _Logger;                        // Compliant
    ILogger log;                            // Compliant

    ILogger _log2;                          // Noncompliant {{Rename this field '_log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    ILogger mylog { get; set; }             // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    //      ^^^^^

    ILogger myLogger, _Log2, _logger;
    //      ^^^^^^^^ {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    //                ^^^^^ @-1 {{Rename this field '_Log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
}
