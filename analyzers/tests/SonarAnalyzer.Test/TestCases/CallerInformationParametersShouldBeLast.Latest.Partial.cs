using System;
using System.Runtime.CompilerServices;

public partial class PartialConstructorCompliantAttributeInImplementation
{
    public partial PartialConstructorCompliantAttributeInImplementation(string other, [CallerFilePath] string callerFilePath = "") { }
};

public partial class PartialConstructorCompliantAttributeInDefinition
{
    public partial PartialConstructorCompliantAttributeInDefinition(string other, [CallerFilePath] string callerFilePath = "") { }
};

public partial class PartialConstructorNonCompliantAttributeInImplementation
{
    public partial PartialConstructorNonCompliantAttributeInImplementation([CallerFilePath] string callerFilePath = "", string other = "") { } // Noncompliant
};

public partial class PartialConstructorNonCompliantAttributeInDefinition
{
    public partial PartialConstructorNonCompliantAttributeInDefinition(string callerFilePath = "", string other = "") { }
};
