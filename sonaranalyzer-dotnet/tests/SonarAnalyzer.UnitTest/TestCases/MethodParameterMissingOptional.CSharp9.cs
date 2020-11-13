using System.Runtime.InteropServices;

void TopLevelMethod([DefaultParameterValue(5)] int j) { } //Noncompliant
void TopLevelOptional([DefaultParameterValue(5), Optional] int j) { }

public record MethodParameterMissingOptional
{
    public void MyMethod1([DefaultParameterValue(5)] int j) { } //Noncompliant
    public void MyMethod2([DefaultParameterValue(5), Optional] int j) { }
    public void MyMethod3([DefaultParameterValue(5)][Optional] int j) { }
    public int this[[DefaultParameterValue(5)] int index] //Noncompliant {{Add the 'Optional' attribute to this parameter.}}
    {
        get => 42;
    }

    public void LocalFunction()
    {
        void Local([DefaultParameterValue(5)] int j) { } //Noncompliant
        void LocalOptional([DefaultParameterValue(5), Optional] int j) { }

        static void StaticLocal([DefaultParameterValue(5)] int j) { } //Noncompliant
        static void StaticLocalOptional([DefaultParameterValue(5), Optional] int j) { }
    }
}
