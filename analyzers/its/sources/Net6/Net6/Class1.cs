// Global Using Directive
using System.Runtime.CompilerServices;

// File-scoped namespace
namespace Csharp10PlayGround;

// Record structs
public record struct RecordStruct(List<string> Property)
{
    public const string Value1 = "value 1";
    public const string Value2 = "Value 2";
    // Constant interpolated strings
    public const string Interpolated = $"{Value1} {Value2}";
}

public struct Struct
{
    // Parameterless constructors and field initializers in structs
    public string Name { get; set; } = "Initialized";
    public string Description { get; set; }

    // Parameterless constructors and field initializers in structs
    public Struct()
    {
        Description = "This is initialized too";
    }
}

public record class RecordClass(int Property)
{
    // Record types can seal ToString
    public sealed override string ToString()
    {
        RecordStruct rs = new RecordStruct(null);
        // Structures as left-hand operands in with expressions
        RecordStruct rs2 = rs with { Property = null };

        // Extended property patterns
//        error AD0001: Analyzer 'SonarAnalyzer.Rules.CSharp.DoNotCheckZeroSizeCollection' threw an exception of type
//        'System.NullReferenceException' with message 'Object reference not set to an instance of an object.'.
//        [C:\SonarSource\sonar - dotnet\analyzers\its\sources\Net6\AdHocExample\AdHocExample.csproj]
//        Exception occurred with following context:
//    Compilation:
//        AdHocExample
//        SyntaxTree: C:\SonarSource\sonar - dotnet\analyzers\its\sources\Net6\AdHocExample\Class1.cs
//        SyntaxNode: { Property.Count: 5 }
//        [PropertyPatternClauseSyntax]@[1107..1128) (38, 31) - (38, 52)
//        System.NullReferenceException: Object reference not set to an instance of an object.
//        at SonarAnalyzer.Rules.CSharp.DoNotCheckZeroSizeCollection.AnalyzePropertyPatternClause(SyntaxNodeAnalysisContext c)


        //if (rs is RecordStruct { Property.Count: 5 }) 
        //{

        //}

        int a = 5;

        // Assignment and declaration in same deconstruction
        (a, int b) = (16, 23);

        var person = new { FirstName = "Scott", LastName = "Hunter", Age = 25 };

        // Extend with expression to anonymous type
        var otherPerson = person with { LastName = "Hanselman" };

        return string.Empty;
    }

    // Allow AsyncMethodBuilder attribute on methods
    [AsyncMethodBuilder(builderType: typeof(RecordClass))]
    public async void SomeMethod()
    {
        //Lambda Attributes 
        Action a =[MyAttribute<int>("asd")] () => { Console.WriteLine("Hello world."); };
        //Allow lambdas with explicit return type and Infer a natural delegate type for lambdas and method groups
        var f = byte () => 5;
    }
}

// Generic Attributes
public class MyAttribute<T> : Attribute
{
    public MyAttribute(string s)
    {

    }
}


// Static abstract members in interfaces
interface I<T> where T : I<T>
{
    static abstract void M();
    static abstract T P { get; set; }
    static abstract event Action E;
    static abstract T operator +(T l, T r);
}

class C : I<C>
{
    static void I<C>.M() => Console.WriteLine("Implementation");
    static C I<C>.P { get; set; }
    static event Action I<C>.E { add { } remove { } }
    static C I<C>.operator +(C l, C r) => r;
}

interface I0
{
    static sealed void M() => Console.WriteLine("Default behavior");

    static sealed int P1 { get; set; }
    static sealed event Action E1;
    static sealed event Action E2 { add => E1 += value; remove => E1 -= value; }

    static sealed I0 operator +(I0 l, I0 r) => l;
}

// Enhanced #line directives and Source Generator V2 APIs examples are missing
