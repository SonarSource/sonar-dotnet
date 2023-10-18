// https://github.com/SonarSource/sonar-dotnet/issues/8083
namespace Repro_8083
{
    using IntMatrix = int[][];

    public class PrimaryConstructors
    {
        public class C1(int[][] a);          // Noncompliant, the ctor is publicly exposed
        public class C2(int[,] a);           // Noncompliant
        public class C3(params int[] a);     // Compliant, params of int
        public class C4(params int[][][] a); // Noncompliant, params of int[][]
        public class C5(int i);              // Compliant, not a multi-dimensional array
    }

    public class Aliases(IntMatrix a)                    // Noncompliant
    {
        public Aliases(IntMatrix a, int i) : this(a) { } // Noncompliant

        public void AMethod1(IntMatrix a) { }            // Noncompliant
        public void AMethod2(params IntMatrix a) { }     // Compliant
    }
}
