using System;

namespace Tests.Diagnostics
{
    public class Web { } // Noncompliant {{Change the name of type 'Web' to be different from an existing framework namespace.}}
//               ^^^
    public enum IO { x };
//              ^^

    public delegate void Runtime(); // Noncompliant
//                       ^^^^^^^

    public interface Linq { } // Noncompliant
//                   ^^^^

    public struct Xaml { } // Noncompliant
//                ^^^^

    interface System { } // Compliant (it's not public)

    private interface Data { } // Error [CS1527] - Compliant (it's not public)

    namespace Accessibility { }  // Compliant (namespace are not checked)

    interface { } // Error [CS1001]

    public class dIrEcToRySeRvIcEs { } // Noncompliant {{Change the name of type 'dIrEcToRySeRvIcEs' to be different from an existing framework namespace.}}
}

namespace All_Variants
{
    public class Accessibility { } // Noncompliant
    public class Activities { } // Noncompliant
    public class Addin { } // Noncompliant
    public class Build { } // Noncompliant
    public class Codedom { } // Noncompliant
    public class Collections { } // Noncompliant
    public class Componentmodel { } // Noncompliant
    public class Configuration { } // Noncompliant
    public class Csharp { } // Noncompliant
    public class Custommarshalers { } // Noncompliant
    public class Data { } // Noncompliant
    public class Dataflow { } // Noncompliant
    public class Deployment { } // Noncompliant
    public class Device { } // Noncompliant
    public class Diagnostics { } // Noncompliant
    public class Directoryservices { } // Noncompliant
    public class Drawing { } // Noncompliant
    public class Dynamic { } // Noncompliant
    public class Enterpriseservices { } // Noncompliant
    public class Globalization { } // Noncompliant
    public class Identitymodel { } // Noncompliant
    public class Interopservices { } // Noncompliant
    public class Io { } // Noncompliant
    public class Jscript { } // Noncompliant
    public class Linq { } // Noncompliant
    public class Location { } // Noncompliant
    public class Management { } // Noncompliant
    public class Media { } // Noncompliant
    public class Messaging { } // Noncompliant
    public class Microsoft { } // Noncompliant
    public class Net { } // Noncompliant
    public class Numerics { } // Noncompliant
    public class Printing { } // Noncompliant
    public class Reflection { } // Noncompliant
    public class Resources { } // Noncompliant
    public class Runtime { } // Noncompliant
    public class Security { } // Noncompliant
    public class Server { } // Noncompliant
    public class Servicemodel { } // Noncompliant
    public class Serviceprocess { } // Noncompliant
    public class Speech { } // Noncompliant
    public class Sqlserver { } // Noncompliant
    public class System { } // Noncompliant
    public class Tasks { } // Noncompliant
    public class Text { } // Noncompliant
    public class Threading { } // Noncompliant
    public class Timers { } // Noncompliant
    public class Transactions { } // Noncompliant
    public class Uiautomationclientsideproviders { } // Noncompliant
    public class Visualbasic { } // Noncompliant
    public class Visualc { } // Noncompliant
    public class Web { } // Noncompliant
    public class Win32 { } // Noncompliant
    public class Windows { } // Noncompliant
    public class Workflow { } // Noncompliant
    public class Xaml { } // Noncompliant
    public class Xamlgeneratednamespace { } // Noncompliant
    public class Xml { } // Noncompliant
}
