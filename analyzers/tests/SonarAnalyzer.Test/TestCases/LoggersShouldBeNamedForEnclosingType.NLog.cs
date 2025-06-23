using NLog;
using static NLog.LogManager;

class EnclosingType
{
    void Method(LogFactory factory)
    {
        LogManager.GetLogger(nameof(EnclosingType));            // Compliant
        LogManager.GetLogger(typeof(EnclosingType).Name);       // Compliant

        LogManager.GetLogger(nameof(AnotherType));              // Noncompliant {{Update this logger to use its enclosing type.}}
        //                          ^^^^^^^^^^^
        LogManager.GetLogger(typeof(AnotherType).Name);         // Noncompliant
        //                          ^^^^^^^^^^^

        factory.GetLogger(nameof(EnclosingType));               // Compliant
        factory.GetLogger(typeof(EnclosingType).Name);          // Compliant

        factory.GetLogger(nameof(AnotherType));                 // Noncompliant {{Update this logger to use its enclosing type.}}
        //                       ^^^^^^^^^^^
        factory.GetLogger(typeof(AnotherType).Name);            // Noncompliant
        //                       ^^^^^^^^^^^

        GetLogger(typeof(AnotherType).Name);            // Noncompliant
        //               ^^^^^^^^^^^
    }
}

class AnotherType : EnclosingType { }
