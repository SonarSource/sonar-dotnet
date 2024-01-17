namespace Tests.Diagnostics
{
    public class Program
    {
        public int GetInt() // Noncompliant {{Remove this method and declare a constant for this value.}}
//                 ^^^^^^
        {
            return 12;
        }

        public int GetMultiplication()
        {
            return 3 * 2;
        }

        public int GetAge(string name) // Compliant - method takes parameters
        {
            return 12;
        }

        private double GetDouble() // Noncompliant
        {
            return (((3.14)));
        }

        private int GetArrow() => 42; // Noncompliant

        public string GetString() // Noncompliant
        {
            return "foo";
        }

        public bool GetIsEnabled()  // Noncompliant
        {
            return true;
        }

        public string GetNull()
        {
            return null;
        }

        public char GetChar() // Noncompliant
        {
            return 'a';
        }

        string GetWithNoModifier() // Noncompliant
        {
            return "";
        }

        public int GetWithInner()
        {
            return GetInner();

            int GetInner() // Compliant - FN - should not be compliant
            {
                return 42;
            }
        }
    }

    public interface IFoo
    {
        int GetValue();
    }

    public class Foo : IFoo
    {
        public int GetValue() // Compliant - implements interface so cannot get rid of the method
        {
            return 42;
        }
    }

    public abstract class Base
    {
        protected virtual string GetName() // Compliant - can be overriden
        {
            return "";
        }

        public abstract float GetPrecision();
    }

    public class NotBase : Base
    {
        protected override string GetName() // Compliant - override
        {
            return "John";
        }

        public override float GetPrecision() // Compliant - override
        {
            return 0.1F;
        }
    }
}
