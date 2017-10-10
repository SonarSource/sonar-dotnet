namespace Tests.TestCases
{
    public class MyClass
    {
        public static double Pi = 3.14;
        public const double Pi2 = 3.14;
        public readonly double Pi3 = 3.14; // Noncompliant {{Make 'Pi3' private.}}
//                             ^^^
        public double Pi4 = 3.14; // Noncompliant
        private double Pi5 = 3.14;
        protected double Pi6 = 3.14;
        internal double Pi7 = 3.14;

        public class MyPublicSubClass
        {
            public double Pi4 = 3.14; // Noncompliant
            private double Pi5 = 3.14;
            protected double Pi6 = 3.14;
            internal double Pi7 = 3.14;
        }

        private class MyPrivateSubClass
        {
            public double Pi4 = 3.14;
            private double Pi5 = 3.14;
            protected double Pi6 = 3.14;
            internal double Pi7 = 3.14;
        }

        protected class MyProtectedSubClass
        {
            public double Pi4 = 3.14;  // Noncompliant
            private double Pi5 = 3.14;
            protected double Pi6 = 3.14;
            internal double Pi7 = 3.14;
        }

        internal class MyInternalSubClass
        {
            public double Pi4 = 3.14;
            private double Pi5 = 3.14;
            protected double Pi6 = 3.14;
            internal double Pi7 = 3.14;
        }
    }

    public struct MyStruct
    {
        public static double Pi = 3.14;
        public const double Pi2 = 3.14;

        public double Pi3;
        private double Pi4;
        internal double Pi5;

        public struct MyPublicSubStruct
        {
            public double Pi4;
            private double Pi5;
            internal double Pi7;
        }

        private struct MyPrivateSubStruct
        {
            public double Pi4;
            private double Pi5;
            internal double Pi7;
        }

        internal struct MyInternalSubStruct
        {
            public double Pi4;
            private double Pi5;
            internal double Pi7;
        }
    }
}