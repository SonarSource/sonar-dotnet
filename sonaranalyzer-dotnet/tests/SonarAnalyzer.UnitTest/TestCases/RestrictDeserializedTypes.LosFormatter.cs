using System;
using System.Web.UI;

namespace SonarAnalyzer.UnitTest.TestCases
{
    internal class RestrictDeserializedTypes
    {
        public void DefaultConstructor()
        {
            new LosFormatter(); // Noncompliant - MAC filtering should be enabled
        }

        public void LiteralExpression()
        {
            new LosFormatter(false, ""); // Noncompliant - MAC filtering should be enabled
            new LosFormatter(false, new byte[0]); // Noncompliant - MAC filtering should be enabled

            new LosFormatter(true, ""); // Compliant - MAC filtering is enabled
            new LosFormatter(true, new byte[0]); // Compliant - MAC filtering is enabled

            new LosFormatter(macKeyModifier: new byte[0], enableMac: false); // Noncompliant
            new LosFormatter(macKeyModifier: new byte[0], enableMac: true); // Compliant
        }

        public void FunctionParameter(bool condition)
        {
            new LosFormatter(condition, ""); // Compliant - unsure about condition value

            if (condition)
            {
                new LosFormatter(condition, ""); // Compliant - MAC filtering is enabled
            }
            else
            {
                new LosFormatter(condition, ""); // Noncompliant - MAC filtering should be enabled
            }
        }

        public void LocalVariables()
        {
            var trueVar = true;
            new LosFormatter(trueVar, ""); // Compliant - MAC filtering is enabled

            var falseVar = false;
            new LosFormatter(falseVar, ""); // Noncompliant - MAC filtering should be enabled
        }

        public void TernaryOp(bool condition)
        {
            var falseVar = condition ? false : false;
            new LosFormatter(falseVar, ""); // Noncompliant - MAC filtering should be enabled

            var trueVar = condition ? true : true;
            new LosFormatter(trueVar, "");

            new LosFormatter(condition ? false : true, "");
        }

        public LosFormatter ExpressionBodyFalse() =>
            new LosFormatter(false, ""); // Noncompliant - MAC filtering should be enabled

        public LosFormatter ExpressionBodyTrue() =>
            new LosFormatter(true, ""); // Compliant - MAC filtering is enabled

        public void InLambdaFunction()
        {
            Func<LosFormatter> createSafe = () => new LosFormatter(true, "");  // Compliant - MAC filtering is enabled
            Func<LosFormatter> createUnsafe = () => new LosFormatter(true, "");  // Compliant - FP: lambda functions are not scanned by symbolic execution
        }

        public LosFormatter SwitchExpression(bool condition)
        {
            return condition switch
            {
                true => new LosFormatter(condition, ""),
                false => new LosFormatter(condition, "") // Compliant - FP: lambda functions are not scanned by symbolic execution
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
                new LosFormatter(condition, ""); // Unreachable
            }

            condition = false;
            if (condition)
            {
                new LosFormatter(condition, ""); // Unreachable
            }
            else
            {
                new LosFormatter(condition, ""); // Noncompliant - MAC filtering should be enabled
            }

            new LosFormatter(new bool(), "");
        }
    }
}
