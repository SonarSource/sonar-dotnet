using System;

public class MyAttribute : Attribute { } // Noncompliant {{Seal this attribute or make it abstract.}}
//           ^^^^^^^^^^^

public class MyOtherAttribute : Attribute { } // Noncompliant

public sealed class Bar : Attribute { } // Compliant - sealed


public abstract class FooBar : Attribute { } // Compliant - abstract

public class NotAnAttribute { } // Compliant - not an attribute

public sealed class Attr : Attribute
{
    private class InnerAttr : Attribute // Compliant - private
    {
        public class InnerInnerAttr : Attribute { } // Compliant - effective accessibility is private
    }

    protected class InnerAttr2 : Attribute { } // Noncompliant
}

public class { }    // Error [CS1001]  Identifier expected
