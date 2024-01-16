using System;

string ToString() { return null; } // Compliant, this is fine since it's not a ToString override

public static class Condition
{
    public static bool When()
    {
        return true;
    }
}

namespace Compliant
{
    class LocalFunctionReturnsNull
    {
        public override string ToString()
        {
            return string.Empty;

            static string Local()
            {
                return null; // Compliant
            }
        }
    }

    class LambdaReturnsNull
    {
        public override string ToString()
        {
            Func<string> expression = () => { return null; }; // Compliant
            Func<string> statment = () => null; // Compliant
            var simple = Simple(s => null); // Compliant
            return string.Empty;
        }

        string Simple(Func<string, string> exp) => exp(null);
    }

    record RecordReturnsStringEmpty
    {
        public override string ToString()
        {
            if (Condition.When()) { return string.Empty; }
            return string.Empty;
        }
    }
}

namespace Noncompliant
{
    record RecordReturnsNull
    {
        public override string ToString()
        {
            if (Condition.When()) { return null; } // Noncompliant
            return null; // Noncompliant
        }
    }
}
