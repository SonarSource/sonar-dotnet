using System;
using System.Web.UI;

internal class RestrictDeserializedTypes
{
    public void DefaultConstructor()
    {
        new LosFormatter();                                                 // FIXME Non-compliant {{Serialized data signature (MAC) should be verified.}}
        new LosFormatter { };                                               // FIXME Non-compliant
    }

    public void LiteralExpression()
    {
        new LosFormatter(false, "");                                        // FIXME Non-compliant {{Serialized data signature (MAC) should be verified.}}
        new LosFormatter(false, new byte[0]);                               // FIXME Non-compliant {{Serialized data signature (MAC) should be verified.}}

        new LosFormatter(true, "");                                         // Compliant - MAC filtering is enabled
        new LosFormatter(true, new byte[0]);                                // Compliant - MAC filtering is enabled

        new LosFormatter(macKeyModifier: new byte[0], enableMac: false);    // FIXME Non-compliant
        new LosFormatter(macKeyModifier: new byte[0], enableMac: true);     // Compliant
    }

    public void FunctionParameter(bool condition)
    {
        new LosFormatter(condition, "");                                    // Compliant - unsure about condition value

        if (condition)
        {
            new LosFormatter(condition, "");                                // Compliant - MAC filtering is enabled
        }
        else
        {
            new LosFormatter(condition, "");                                // FIXME Non-compliant - MAC filtering should be enabled
        }
    }

    public void LocalVariables()
    {
        var trueVar = true;
        new LosFormatter(trueVar, "");                                      // Compliant - MAC filtering is enabled

        var falseVar = false;
        new LosFormatter(falseVar, "");                                     // FIXME Non-compliant - MAC filtering should be enabled
    }

    public void TernaryOp(bool condition)
    {
        var falseVar = condition ? false : false;
        new LosFormatter(falseVar, "");                                     // FIXME Non-compliant - MAC filtering should be enabled

        var trueVar = condition ? true : true;
        new LosFormatter(trueVar, "");

        new LosFormatter(condition ? false : true, "");
    }

    public LosFormatter ExpressionBodyFalse() =>
        new LosFormatter(false, "");                                        // FIXME Non-compliant - MAC filtering should be enabled

    public LosFormatter ExpressionBodyTrue() =>
        new LosFormatter(true, "");                                         // Compliant - MAC filtering is enabled

    public void InLambdaFunction()
    {
        Func<LosFormatter> createSafe = () => new LosFormatter(true, "");   // Compliant - MAC filtering is enabled
        Func<LosFormatter> createUnsafe = () => new LosFormatter(true, ""); // Compliant - FN: lambda functions are not scanned by symbolic execution
    }

    public LosFormatter Switch(bool condition)
    {
        switch (condition)
        {
            case true:
                return new LosFormatter(condition, "");
            default:
                return new LosFormatter(condition, "");                     // Compliant - FN: lambda functions are not scanned by symbolic execution
        };
    }

    public void DataFlow()
    {
        var condition = false;
        condition = true;
        if (condition)
        {
            new LosFormatter(condition, "");
        }
        else
        {
            new LosFormatter(condition, "");                                // Unreachable
        }

        condition = false;
        if (condition)
        {
            new LosFormatter(condition, "");                                // Unreachable
        }
        else
        {
            new LosFormatter(condition, "");                                // FIXME Non-compliant - MAC filtering should be enabled
        }

        new LosFormatter(new bool(), "");
    }
}
