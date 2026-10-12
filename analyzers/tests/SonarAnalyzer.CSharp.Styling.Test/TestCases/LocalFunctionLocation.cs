using System;

class Noncompliant
{
    public string Value
    {
        get
        {
            string LocalFunction() => "Empty"; // Noncompliant {{This local function should be at the end of the method.}}
            return LocalFunction();
        }
        set
        {
            static void LocalFunction(string _) { }; // Noncompliant {{This local function should be at the end of the method.}}
            LocalFunction(value);
        }
    }

    public string this[int a] {
        get
        {
            string LocalFunction() => "Empty"; // Noncompliant {{This local function should be at the end of the method.}}
            return LocalFunction();
        }
        set
        {
            static void LocalFunction(string _) { } // Noncompliant {{This local function should be at the end of the method.}}
            LocalFunction(value);
        }
    }

    public Noncompliant()
    {
        void LocalFunction() { } // Noncompliant {{This local function should be at the end of the method.}}
        //   ^^^^^^^^^^^^^

        LocalFunction();
    }

    void SingleLocalFunction()
    {
        void LocalFunction() { } // Noncompliant {{This local function should be at the end of the method.}}
        //   ^^^^^^^^^^^^^

        LocalFunction();
    }

    void StaticLocalFunction()
    {
        static int LocalFunction() => 42; // Noncompliant

        LocalFunction();
    }

    void MultipleLocalFunctions()
    {
        LocalFunction();

        void LocalFunction() { } // Noncompliant

        LocalFunction2();

        void LocalFunction2() { } // Noncompliant {{This local function should be at the end of the method.}}

        LocalFunction();
    }

    void WithinBlock(bool a)
    {
        if (a)
        {
            LocalFunction();
            void LocalFunction() { } // Noncompliant
        }
    }

    void MixCompliantNoncompliant()
    {
        LocalFunction();

        void LocalFunction() { } // Noncompliant

        LocalFunction2();
        LocalFunction();

        void LocalFunction2() { }
    }

    void LocalFunctionInLambdaNotLast()
    {
        Action a = () =>
        {
            void LocalFunction() { } // Noncompliant

            LocalFunction();
        };
    }

    void LocalFunctionInSimpleLambdaNotLast()
    {
        Action<int> a = x =>
        {
            void LocalFunction() { } // Noncompliant

            LocalFunction();
        };
    }

    void LocalFunctionInAnonymousMethodNotLast()
    {
        Action a = delegate
        {
            void LocalFunction() { } // Noncompliant

            LocalFunction();
        };
    }

    void LocalFunctionInNestedLocalFunctionNotLast()
    {
        void Outer() // Noncompliant
        {
            void Inner() { } // Noncompliant

            Inner();
        }

        Outer();
    }
}

class Compliant
{
    public string Value
    {
        get
        {
            return LocalFunction();
            string LocalFunction() => "Empty";
        }
        set
        {
            LocalFunction(value);
            static void LocalFunction(string _)
            { }
        }
    }

    public string this[int a]
    {
        get
        {
            return LocalFunction();
            string LocalFunction() => "Empty";
        }
        set
        {
            LocalFunction(value);
            static void LocalFunction(string _)
            { }
        }
    }

    public Compliant()
    {
        LocalFunction();

        void LocalFunction()
        { }
    }

    void SingleLocalFunction()
    {
        LocalFunction();

        void LocalFunction() { }
    }

    void WithinBlock(bool a)
    {
        if (a)
        {
            LocalFunction();
        }
        void LocalFunction() { }
    }

    void MultipleLocalFunctions()
    {
        LocalFunction();
        LocalFunction();
        LocalFunction2();

        void LocalFunction() { }
        void LocalFunction2() { }
    }

    void LocalFunctionInLambda()
    {
        Action a = () =>
        {
            LocalFunction();

            void LocalFunction() { }
        };
    }

    void LocalFunctionInSimpleLambda()
    {
        Action<int> a = x =>
        {
            LocalFunction();

            void LocalFunction() { }
        };
    }

    void LocalFunctionInAnonymousMethod()
    {
        Action a = delegate
        {
            LocalFunction();

            void LocalFunction() { }
        };
    }

    void LocalFunctionInNestedLocalFunction()
    {
        Outer();

        void Outer()
        {
            Inner();

            void Inner() { }
        }
    }
}
