using System;

public abstract class Sample
{
    public abstract void Empty(); //Compliant
    public abstract void SingleParam(int param); //Compliant
    public abstract void DoubleParam1(int a, int b); //Compliant
    public abstract void DoubleParam2(int a, 
        int b); //Compliant
    public abstract void DoubleParam3(
        int a,
        int b); //Compliant

    public abstract void CompliantSingleLineMethod(string first, string second, string third, string fourth); // Compliant

    public abstract void CompliantMultiLineMethod(string first, // Compliant
        string second,
        string third,
        string fourth);

    public abstract void NonCompliantMethod(string first, string second, string third,
                               string fourth); // Noncompliant {{Parameters should be on the same line or all on separate lines.}}

    public abstract void NonCompliantMethodStart(string first, string second,
        string third, // Noncompliant
    //  ^^^^^^^^^^^^
        string fourth);

    public abstract void NonCompliantMethodMiddle(string first,
                                        string second, string third,    // Noncompliant
//                                                     ^^^^^^^^^^^^
                                        string fourth);

    public abstract void NonCompliantMethodEnd(string first,
        string second,
        string third, string fourth); // Noncompliant
    //                ^^^^^^^^^^^^^

    public abstract void Foo(
    string first, // Compliant
    string second,
    string third,
    string fourth);

    public abstract void Foo1(
     string first
     , string second, string third);  // Noncompliant

    public abstract void Foo2( // Compliant
        string first
        , string second,
        string third);

    public abstract void Foo3(string first
        , string second, string third);  // Noncompliant

    public abstract void Foo4(string first // Compliant
        , string second,
        string third);

    public abstract void Foo5( // Compliant
    string first
    ,
    string second
    ,
    string third);

    public abstract void Foo6( // Compliant
    string first
    ,
    string second
    , string third,
    string fourth);

    public abstract void Foo7(
    string first //compliant


    ,


    string second
    , string third,
    string fourth);

    //DelegateDeclarationSyntax
    public delegate void Compliant(int a, int b, int c);
    public delegate void Noncompliant(int a, int b,
        int c); // Noncompliant

    //ParenthesizedLambdaExpressionSyntax
    Func<int, int, int> pleC = (int x, int y) => x + y; //Compliant
    Func<int,
        int, int, int> pleNC = (int x, int y
        , int z) => x + y + z; //Noncompliant

    //AnonymousMethodExpression
    public void Ame()
    {
        Action<int, int, int> ame = delegate (int x, int y, int z) //Compliant
        { };
        ame = delegate (int x, int y,
            int z) //Noncompliant
        { };
    }

    //LocalFunctionStatementSyntax
    public void lfs()
    {
        void localFS(int x, int y, int z) //Compliant
        { };

        void localFSNoncompliant(int x,
            int y, int z) //Noncompliant
        { };
    }

    public abstract void EndOfLineInParameter(int x, string y, params //Noncompliant
    int
    []
    otherParams);

    public abstract void EndOfLineEndParameters(int x, string y, params int[] otherParams //Compliant
        );

    public abstract void CommaInParams1((int,
int) x, int y, int z);  //Compliant

    public abstract void CommaInParams2(
    Action<int,
    int> a, int b, int c); //Compliant

    public abstract void CommaInParams3((int,
        int) x, int y,
        int z); //Noncompliant

    public abstract void CommaInParams4(
        Action<int,
        int> a, int b
            , int c); //Noncompliant

    public abstract void CommaInParams5(
        Action<int,
        int> a,
        int b, int c); //Noncompliant

    public abstract void CommaInParams6(
        Func<string,
        int> a, Action<int,
        int> b
            , string c = "Hello"); //Compliant
}
//FunctionPointerSyntax
public unsafe class Example
{
    public delegate*<int, int, int> Compliant; //Compliant
    public delegate*<int,
        int, int> Noncompliant; //Noncompliant
}

//ClassDeclarationSyntax
public class cdsC(int a, int b, int c) { } //Compliant
public class cdsNC(int a,
    int b, int c) //Noncompliant
{ } 

//StructDeclarationSyntax
public struct sdsC(int a, int b, int c) { } //Compliant
public struct sdsNC(int a,
    int b, int c) //Noncompliant
{ } 

//InterfaceDeclarationSyntax
public interface ICompliant<T, X, Y> //Compliant
{ }
public interface INoncompliant<T,
    X, Y> //Noncompliant
{ }

//RecordDeclarationSyntax
public record rdsC(int a, int b, int c); //Compliant
public record rdsNC(int a, int b,
    int c); //Noncompliant

//ConstructorDeclarationSyntax
public abstract class Constructor
{
    public abstract void Compliant(
        int a,
        int b,
        int c,
        int d);

    public abstract void Noncompliant(
        int a,
        int b,
        int c, int d); //Noncompliant
}

//IndexerDeclarationSyntax
public class idsC
{
    public int this[int a, int b, int c] //Compliant
    {
        get
        {
            return a;
        }
        set { }
    }
}

public class idsNC
{
    public int this[int a,
        int b, int c] //Noncompliant
    {
        get
        {
            return a;
        }
        set { }
    }
}
