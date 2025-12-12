using System;

public partial class VariableShadowsField
{
    public void Method(object someParameter)
    {
        int myField = 0; // Noncompliant
        //  ^^^^^^^
        int myProperty = 0; // Noncompliant
        //  ^^^^^^^^^^
    }
}

public partial class VariableShadowsFieldPrimaryConstructor
{
    void Method()
    {
        var a = 0; // Noncompliant
        //  ^
    }
}

public partial class PartialProperty
{

    public partial int myPartialProperty { get => 42; set { } }

    public void DoSomething()
    {
        int myPartialProperty = 0;  // Noncompliant
    }

}

public partial class PartialConstructor
{
    public partial PartialConstructor()
    {
        int field = 42;             // Noncompliant
    }
}

public partial class PartialEvent
{
    public partial event EventHandler myPartialEvent
    {
        add { int field = 0;  }     // Noncompliant
        remove { int field = 0; }   // Noncompliant
    }
    public void Shadow()
    {
        int myPartialEvent = 0;     // FN https://sonarsource.atlassian.net/browse/NET-2846
    }
}
