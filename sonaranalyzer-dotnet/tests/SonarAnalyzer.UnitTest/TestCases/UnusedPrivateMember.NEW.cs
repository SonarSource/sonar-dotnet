namespace Tests.Diagnostics
{
    using System;
    using System.ComponentModel;

    public class MyAttribute : Attribute { }

    public class MethodAccessibility
    {
        private int PrivateMethod() { } // Noncompliant {{Remove the unused private method 'PrivateMethod'.}}
        private static int PrivateStaticMethod() { } // Noncompliant

        // not private Methods are compliant
        internal int InternalMethod() { }
        protected int ProtectedMethod() { }
        protected internal int ProtectedInternalMethod() { }
        public int PublicMethod() { }
        internal static int InternalStaticMethod() { }
        protected static int ProtectedStaticMethod() { }
        protected static internal int ProtectedInternalStaticMethod() { }
        public static int PublicStaticMethod() { }
    }

    public class MethodUsages
    {
        private int Method1() { }
        private int Method2() { }
        private int Method3() { }
        private int Method4() { }
        private int Method5() { }
        private int Method6() { }
        private int Method7() { }
        private int Method8() { }
        private int Method9() { }
        private int Method10() { }
        [My]
        private int Method11() { } // Methods with attributes are not removable

        public void SomeMethod()
        {
            int value;

            value = Method1();

            value = ((this.Method2()));

            Console.Write(this?.Method3());

            Func<int> x = () => Method4();

            return Method5();

            var o = new { Method6() };

            DoSomethingWithMethod(Method7);
        }

        public void ExpressionBodyMethod() => Method8();

        public int Property { get; set; } = Method9();

        public MethodUsages(int number) { }

        public MethodUsages() : this(Method10()) { }

        public void DoSomethingWithMethod(Func<int> func) { }
    }
}
