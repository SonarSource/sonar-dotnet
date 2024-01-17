namespace Tests.Diagnostics
{
    class Program
    {
        private int[,] multiDimArrayField = // Noncompliant {{Change this multidimensional array to a jagged array.}}
//                     ^^^^^^^^^^^^^^^^^^
            {
                {1,2,3,4},
                {5,6,7,0},
                {8,0,0,0},
                {9,0,0,0}
            };

        private int[][] jaggedArrayField = // Compliant
            {
                new int[] {1,2,3,4},
                new int[] {5,6,7},
                new int[] {8},
                new int[] {9}
            };

        public Program(string[,] array) // Noncompliant
        { }

        public float[,] Foo() // Noncompliant
        {
            return null;
        }

        public void Bar(string[,] multiDimArrayParameter) // Noncompliant
        {
        }

        public void FooBar()
        {
            string[,] multiDimArrayVar; // Noncompliant
            string[][] jaggedArrayVar;
        }

        public int[,] MultiDimArrayProperty { get; set; } // Noncompliant

        public int Prop
        {
            get
            {
                string[,] multiDimArrayPropVar; // Noncompliant
                return 0;
            }
        }
    }
}
