string LocalFunction() => "Empty"; // Noncompliant {{This local function should be at the end of the method.}}
//     ^^^^^^^^^^^^^

_ = LocalFunction();

string LocalFunction2() => "Empty";
static string StaticLocalFunction() => "Empty";

class MyClass
{
    public void Method()
    {
        void LocalFunction() { } // Noncompliant

        LocalFunction();
    }
}
