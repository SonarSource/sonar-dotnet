﻿public partial class VariableShadowsField
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
