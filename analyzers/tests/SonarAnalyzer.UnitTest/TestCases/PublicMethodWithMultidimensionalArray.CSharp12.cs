// https://github.com/SonarSource/sonar-dotnet/issues/8083
namespace Repro_8083
{
    using IntMatrix = int[][];

    public class PrimaryConstructors
    {
        public class C1(int[][] a);          // FN, the ctor is publicly exposed
        public class C2(int[,] a);           // FN
        public class C3(params int[] a);     // Compliant, params of int
        public class C4(params int[][][] a); // FN, params of int[][]
        public class C5(int i);              // Compliant, not a multi-dimensional array
    }

    public class Aliases(IntMatrix a)                    // FN
    {
        public Aliases(IntMatrix a, int i) : this(a) { } // FN

        public void AMethod1(IntMatrix a) { }            // Noncompliant
        public void AMethod2(params IntMatrix a) { }     // Compliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8100
namespace Repro_8100
{
    public class InlineArrays
    {
        public void AMethod1(Buffer[] a) { }          // FN, Buffer[] is 2-dimensional
        public void AMethod1(Buffer[,] a) { }         // Noncompliant, Buffer[,] is 3-dimensional
        public void AMethod2(params Buffer[] a) { }   // Compliant, params of Buffer
        public void AMethod3(params Buffer[][] a) { } // FN, params of Buffer[]
    }

    [System.Runtime.CompilerServices.InlineArray(10)]
    public struct Buffer
    {
        int arrayItem;
    }
}
