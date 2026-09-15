using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

public class Program
{
    string mylogger;                        // Compliant
    ILog log;                               // Compliant
    ILogger _Logger;                        // Compliant
    Logger _log;                            // Compliant

    ILogger _log2;                          // Noncompliant {{Rename this field '_log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    ILog my_logger;                         // Noncompliant {{Rename this field 'my_logger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    Logger mylog { get; set; }              // Noncompliant {{Rename this property 'mylog' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    //     ^^^^^

    MyLogger myLogger, _Log2, _logger;
    //       ^^^^^^^^ {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    //                 ^^^^^ @-1 {{Rename this field '_Log2' to match the regular expression '^_?[Ll]og(ger)?$'.}}
}
public class MyLogger : Logger
{
    public MyLogger(string name) : base(name) { }
}
