using System;

namespace Tests.Diagnostics
{
    public class MyInvalidAttribute : Attribute
//               ^^^^^^^^^^^^^^^^^^ {{Specify AttributeUsage on 'MyInvalidAttribute'.}}
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MyCompliantAttribute : Attribute
    {
    }

    public class MyInvalidInheritedAttribute : MyCompliantAttribute
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Specify AttributeUsage on 'MyInvalidInheritedAttribute' to improve readability, even though it inherits it from its base type.}}
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MyInheritedAttribute : MyCompliantAttribute
    {
    }
}
