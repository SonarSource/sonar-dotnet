namespace Tests.TestCases
{
    public interface IWithDefaultImplementation
    {
        decimal Count { get; set; }
        decimal Price { get; set; }

        void Reset(int a);     //Compliant

        //Default interface methods
        decimal Total()
        {
            return Count * Price;
        }

        decimal Total(decimal Discount)
        {
            return Count * Price * (1 - Discount);
        }

        decimal Total(string unused)    // Compliant, because it's interface member
        {
            return Count * Price;
        }
    }

    public class StaticLocalFunctions
    {
        public int DoSomething(int a)   // Compliant
        {
            static int LocalFunction(int x, int seed) => x + seed;
            static int BadIncA(int x, int seed) => x + 1;   //Noncompliant
            static int BadIncB(int x, int seed)             //Noncompliant
            {
                seed = 1;
                return x + seed;
            }
            static int BadIncRecursive(int x, int seed)     //Noncompliant
            {
                seed = 1;
                if (x > 1)
                {
                    return BadIncRecursive(x - 1, seed);
                }
                return x + seed;
            }

            return LocalFunction(a, 42) + BadIncA(a, 42) + BadIncB(a, 42) + BadIncRecursive(a, 42);
        }
    }

    public class SwitchExpressions
    {
        public int DoSomething(int a, bool b)
        {
            return b switch
            {
                true => a,
                _ => 0
            };
        }
    }

    public class UsageInRange
    {
        public void DoSomething(int a, int b)
        {
            var list = new string[] { "a", "b", "c" };
            var sublist = list[a..];
            System.Range r = ..^b;
            sublist = list[r];
        }
    }
}
