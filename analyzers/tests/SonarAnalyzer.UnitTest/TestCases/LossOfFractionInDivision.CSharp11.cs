using System;

namespace Testcases
{
    class NativeIntChecks 
    {
        decimal result;
        Func<decimal> decimalFunc;

        IntPtr intPtr1 = 2;
        IntPtr intPtr2 = 3;

        UIntPtr uIntPtr1 = 2;
        UIntPtr uIntPtr2 = 3;

        nint nint1 = 2;
        nint nint2 = 3;

        nuint nuint1 = 2;
        nuint nuint2 = 3;

        void AssignmentChecks()
        {
            result = intPtr1 / intPtr2; // Noncompliant
            result = uIntPtr1 / uIntPtr2; // Noncompliant
            result = nint1 / nint2;  // Noncompliant
            result = nuint1 / nuint2; // Noncompliant
            result = intPtr1 / nint1; // Noncompliant
            result = nint1 / intPtr1; // Noncompliant
            result = uIntPtr1 / nuint1; // Noncompliant
            result = nuint1 / uIntPtr1; // Noncompliant

            result = (decimal)intPtr1 / intPtr2; // Compliant
            result = (decimal)uIntPtr1 / uIntPtr2; // Compliant
            result = (decimal)nint1 / nint2;  // Compliant
            result = (decimal)nuint1 / nuint2; // Compliant
            result = (decimal)intPtr1 / nint1; // Compliant
            result = (decimal)nint1 / intPtr1; // Compliant
            result = (decimal)uIntPtr1 / nuint1; // Compliant
            result = (decimal)nuint1 / uIntPtr1; // Compliant
        }

        void MethodArgumentChecks()
        {
            MethodAcceptingDecimal(intPtr1 / intPtr2); // Noncompliant
            MethodAcceptingDecimal(uIntPtr1 / uIntPtr2); // Noncompliant
            MethodAcceptingDecimal(nint1 / nint2);  // Noncompliant
            MethodAcceptingDecimal(nuint1 / nuint2); // Noncompliant
            MethodAcceptingDecimal(intPtr1 / nint1); // Noncompliant
            MethodAcceptingDecimal(nint1 / intPtr1); // Noncompliant
            MethodAcceptingDecimal(uIntPtr1 / nuint1); // Noncompliant
            MethodAcceptingDecimal(nuint1 / uIntPtr1); // Noncompliant

            MethodAcceptingDecimal((decimal)intPtr1 / intPtr2); // Compliant
            MethodAcceptingDecimal((decimal)uIntPtr1 / uIntPtr2); // Compliant
            MethodAcceptingDecimal((decimal)nint1 / nint2);  // Compliant
            MethodAcceptingDecimal((decimal)nuint1 / nuint2); // Compliant
            MethodAcceptingDecimal((decimal)intPtr1 / nint1); // Compliant
            MethodAcceptingDecimal((decimal)nint1 / intPtr1); // Compliant
            MethodAcceptingDecimal((decimal)uIntPtr1 / nuint1); // Compliant
            MethodAcceptingDecimal((decimal)nuint1 / uIntPtr1); // Compliant

            decimal MethodAcceptingDecimal(decimal arg) => arg;
        }

        void FuncReturnChecks()
        {
            decimalFunc = () => intPtr1 / intPtr2; // Noncompliant
            decimalFunc = () => uIntPtr1 / uIntPtr2; // Noncompliant
            decimalFunc = () => nint1 / nint2;  // Noncompliant
            decimalFunc = () => nuint1 / nuint2; // Noncompliant
            decimalFunc = () => intPtr1 / nint1; // Noncompliant
            decimalFunc = () => nint1 / intPtr1; // Noncompliant
            decimalFunc = () => uIntPtr1 / nuint1; // Noncompliant
            decimalFunc = () => nuint1 / uIntPtr1; // Noncompliant

            decimalFunc = () => (decimal)intPtr1 / intPtr2; // Compliant
            decimalFunc = () => (decimal)uIntPtr1 / uIntPtr2; // Compliant
            decimalFunc = () => (decimal)nint1 / nint2;  // Compliant
            decimalFunc = () => (decimal)nuint1 / nuint2; // Compliant
            decimalFunc = () => (decimal)intPtr1 / nint1; // Compliant
            decimalFunc = () => (decimal)nint1 / intPtr1; // Compliant
            decimalFunc = () => (decimal)uIntPtr1 / nuint1; // Compliant
            decimalFunc = () => (decimal)nuint1 / uIntPtr1; // Compliant
        }

        decimal MethodReturnChecks()
        {
            return intPtr1 / intPtr2; // Noncompliant
            return uIntPtr1 / uIntPtr2; // Noncompliant
            return nint1 / nint2;  // Noncompliant
            return nuint1 / nuint2; // Noncompliant
            return intPtr1 / nint1; // Noncompliant
            return nint1 / intPtr1; // Noncompliant
            return uIntPtr1 / nuint1; // Noncompliant
            return nuint1 / uIntPtr1; // Noncompliant

            return (decimal)intPtr1 / intPtr2; // Compliant
            return (decimal)uIntPtr1 / uIntPtr2; // Compliant
            return (decimal)nint1 / nint2;  // Compliant
            return (decimal)nuint1 / nuint2; // Compliant
            return (decimal)intPtr1 / nint1; // Compliant
            return (decimal)nint1 / intPtr1; // Compliant
            return (decimal)uIntPtr1 / nuint1; // Compliant
            return (decimal)nuint1 / uIntPtr1; // Compliant
        }
    }
}

