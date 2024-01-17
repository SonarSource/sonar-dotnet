public static class Condition
{
    public static bool When()
    {
        return true;
    }
}

namespace Compliant
{
    class OtherMethodReturnsNullString
    {
        string Returns()
        {
            return null;
        }
    }

    class ReturnsSomeString
    {
        public override string ToString()
        {
            if (Condition.When())
            {
                return "Hello, world!";
            }
            return "Hello, world!";
        }
    }

    class ReturnsEmptyString
    {
        public override string ToString()
        {
            if (Condition.When())
            {
                return "";
            }
            return "";
        }
    }

    class ReturnsStringEmpty
    {
        public override string ToString()
        {
            if (Condition.When())
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }

    struct ReturnsStringEmptyStruct
    {
        public override string ToString()
        {
            if (Condition.When()) { return string.Empty; }
            return string.Empty;
        }
    }

    class ToString
    {
        public string SomeMethod()
        {
            return null; // Compliant
        }
    }
}

class Noncompliant
{
    public class ReturnsNull
    {
        public override string ToString()
        {
            if (Condition.When())
            {
                return null; // Noncompliant
            //  ^^^^^^^^^^^^
            }
            return null; // Noncompliant {{Return an empty string instead.}}
        }
    }

    public class ReturnsNullConditionaly
    {
        public override string ToString()
        {
            if (Condition.When())
            {
                return null; // Noncompliant
            }
            return "not-null";
        }
    }

    public class ReturnsNullViaExpressionBody
    {
        public override string ToString() => null; // Noncompliant
    }

    public class ReturnsNullViaTernaryExpressionBody
    {
        public override string ToString() => Condition.When() ? null : string.Empty; // Noncompliant
    }

    public class ReturnsNullViaTernary
    {
        public override string ToString()
        {
            return Condition.When() ? null : ""; // Noncompliant
        }
    }

    public class ReturnsNullViaNestedTenary
    {
        public override string ToString() => // Noncompliant
            Condition.When()
             ? (Condition.When() ? null : "something") 
             : (Condition.When() ? "something" : null);
    }

    struct StructReturnsNull
    {
        public override string ToString()
        {
            if (Condition.When()) { return null; } // Noncompliant
            return null; // Noncompliant
        }
    }

    public class SomeClass
    {
        public static string ToString()
        {
            return null; // Compliant
        }
    }
}
