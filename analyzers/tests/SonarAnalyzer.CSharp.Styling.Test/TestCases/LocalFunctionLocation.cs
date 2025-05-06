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
}

class Compliant
{
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
}
