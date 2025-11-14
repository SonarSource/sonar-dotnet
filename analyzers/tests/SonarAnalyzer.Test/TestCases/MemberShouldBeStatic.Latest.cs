using System;

int TopLevelLocalfunction() => 0;       // Compliant, local functions are not class members

var localVariable = 42;
int LocalFunction() => localVariable;   // Compliant

namespace CSharp8
{
    public interface IUtilities
    {
        public int MagicNum { get { return 42; } } // Compliant, inside interface

        private static string magicWord = "please";

        public string MagicWord // Compliant, inside interface
        {
            get { return magicWord; }
            set { magicWord = value; }
        }

        public int Sum(int a, int b) // Compliant, inside interface
        {
            return a + b;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3204
    public class Repro_3204<TFirst, TSecond>
    {
        private protected int ProtectedInternal() => 42;    // Noncompliant, not accessible from outside this class
    }
}

namespace CSharp9
{
    public record Methods
    {
        private int instanceMember;
        private static int staticMember;

        public int Method1() => 0;                              // Noncompliant {{Make 'Method1' a static method.}}
        public int Method2() => instanceMember;
        public int Method3() => this.instanceMember;
        public int Method4() => staticMember;                   // Noncompliant
        public int Method5() => Methods.staticMember;           // Noncompliant
        public int Method6() => new Methods().instanceMember;   // Noncompliant
        public int Method7(Methods arg) => arg.instanceMember;  // Noncompliant

        public static int StaticMethod1() => 0;
        public static int StaticMethod2() => staticMember;
    }

    public class Lambda
    {
        private int instanceMember;
        private static int staticMember;

        public void Method()
        {
            var variable = 0;
            Execute(() => instanceMember);
            Execute(() => variable);

            // These lambdas could be static, but the rule doesn't apply to lambdas
            Execute(() => 0);
            Execute(() => staticMember);
        }

        private void Execute(Func<int> f) { }
    }
}

namespace CSharp12
{
    public class PrimaryClass(SomeService s1, SomeService s2, SomeService s3)
    {
        public SomeService s1 = s1;
        public SomeService s2 { get; set; } = s2;

        private void Access_Parameter_OfPrimaryConstructor(int a, int b) // Compliant
        {
            _ = s1.Sum(a, b);
            _ = s2.Sum(a, b);
            _ = s3.Sum(a, b);
        }

        private void NoAccessTo_Parameter_OfPrimaryConstructor(int a, int b) // Noncompliant
        {
            _ = a + b;
        }

        private void MethodParameter_Hides_Parameter_OfPrimaryConstructor(int s1, int s2) // Noncompliant
        {
            _ = s1 + s2;
        }
    }

    public record PrimaryRecord(SomeService s1, SomeService s2, SomeService s3)
    {
        public SomeService s1 = s1;
        public SomeService s2 { get; set; } = s2;

        private void Access_Parameter_OfPrimaryConstructor(int a, int b) // Compliant
        {
            _ = s1.Sum(a, b);
            _ = s2.Sum(a, b);
            _ = s3.Sum(a, b);
        }

        private void NoAccessTo_Parameter_OfPrimaryConstructor(int a, int b) // Noncompliant
        {
            _ = a + b;
        }

        private void MethodParameter_Hides_Parameter_OfPrimaryConstructor(int s1, int s2) // Noncompliant
        {
            _ = s1 + s2;
        }
    }


    public class SomeService
    {
        private int x = 42;
        public int Sum(int a, int b) => a + b + x;
    }
}

namespace CSharp13
{
    public partial class Partial
    {
        public partial int Prop { get; }
    }
}

namespace CSharp14
{
    public static class ReproNET2621 // https://sonarsource.atlassian.net/browse/NET-2621
    {
        extension(int number)
        {
            public bool IsEven => number % 2 == 0; //   Noncompliant FP, number is instance data.
            public bool IsOdd() => number % 2 != 0; //  Noncompliant FP, number is instance data.
        }
    }
}
