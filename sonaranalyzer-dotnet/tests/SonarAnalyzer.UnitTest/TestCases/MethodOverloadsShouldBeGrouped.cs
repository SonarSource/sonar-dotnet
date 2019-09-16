namespace Tests.Diagnostics
{
    class A
    {
        void a() { } // Noncompliant {{All 'a' signatures should be adjacent}}
//           ^

        void a(int a, char b) { }

        class B { }

        void a(string a) { } // Secondary
//           ^

        interface C { }

        void a(int a) { } // Secondary {{Non-adjacent overload}}
//           ^

        enum D { }

        void a(double a) { } // Secondary {{Non-adjacent overload}}
//           ^

        int myField;

        void a(char a) { } // Secondary {{Non-adjacent overload}}
//           ^

        int MyProperty
        {
            get;
        }

        void a(char a, int b) { } // Secondary {{Non-adjacent overload}}
//           ^
    }

    class B
    {
        B() { } // Noncompliant {{All 'B' signatures should be adjacent}}

        B(int a) { }

        B(char a) { }

        ~B() { }

        B(double a) { } // Secondary {{Non-adjacent overload}}
    }

    class C
    {
        public static C operator -(C c1) // Compliant - this one is unary -, while the other is binary -
        {
            return null;
        }

        void a() { }

        public static implicit operator B(C d) => null; // Compliant - this operator and the B() method are not related

        void B() { }

        public static C operator -(C c1, C c2)
        {
            return null;
        }
    }

    interface D
    {
        void DoSomething(double b);

        void DoSomething();

        void DoSomething(int a);
    }

    interface E
    {
        void DoSomething(double b); // Noncompliant {{All 'DoSomething' signatures should be adjacent}}
//           ^^^^^^^^^^^

        void DoSomething();

        void DoSomethingElse();

        void DoSomething(int a); // Secondary {{Non-adjacent overload}}
//           ^^^^^^^^^^^
    }

    struct F
    {
        F(char a) { }

        F(int a) { }

        void MyStructMethod() { } // Noncompliant {{All 'MyStructMethod' signatures should be adjacent}}

        void MyStructMethod(int a) { }

        void MyStructMethod2() { }

        void MyStructMethod(char a) { } // Secondary {{Non-adjacent overload}}
    }
}
