# Coding Style

## General

When contributing to the project, and if otherwise not mentioned in this document, our coding conventions
follow the Microsoft [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
and standard [Naming Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines).

## Class Members

Members and types should always have the lowest possible visibility.

Ordering of class members should be the following:

1. Constants
1. Nested enum declarations
1. Fields
1. Abstract members
1. Properties
1. Constructors
1. Methods 
1. Nested types

Furthermore, each of these categories should be ordered from higher to lower accessibility level (public, internal, protected, private).

Static fields and properties should be placed before instance ones. 

Static methods are preferred to be after instance methods.

Once grouped as specified above, methods which are called by other methods in the same group should be placed below the callers.

```csharp
public int PublicMethod() => 42;

int CallerOne() => Leaf();

int CallerTwo() => Leaf() + PublicMethod();

int Leaf() =>  42;
```

Do not use auto-implemented private properties. Use fields instead.

### Local functions

There are no strict rules on when to use local functions. It should be decided on a case-by-case basis.

By default, you should prefer methods over local functions. Use local functions if it makes the code significantly easier to understand. For example:
- Accessing the method's local state directly, instead of using parameters, reduces noise.
- The name of the function would not make sense at the class level.

Local functions should always be placed at the end of a method.

```csharp
public int MethodWithLocalFunction(int x)
{
    return LocalFunction(x);
    
    int LocalFunction(int x) => x;
}
```

### Separation

Individual members must be separated by empty line, except sequence of constants, fields, single-line properties and abstract members. These members should not be separated by empty lines.

```csharp
private const int ValueA = 42;
private const int ValueB = 24;

private int valueA;
private int valueB;

protected abstract int AbstractA { get; }
protected abstract void AbstractB();

public SemanticModel Model { get; }
public SyntaxNode Node { get; }

public int ComplexProperty
{
    get => 42;
    set
    {
        // ...
    }
}

public Constructor() { }

public void MethodA() =>
    MethodB();

public void MethodB()
{
    // ...
}
```

## Naming conventions

### Principles

Keep it minimal and suggestive.
- Generic words that don't convey meaning (e.g. `Helper`) should be avoided.
- Overwordy and complex names should be avoided as well.
- Use positive naming when possible.

### Casing

Protected fields should start with lowercase letter.

### Parameters and variables

Single variable lambdas should use `x` as the variable name (based on lambda calculus λx). Multi variable lambdas should use descriptive names, where `x` can be used for the main iterated item like `(x, index) => ...`. Name `c` can be used for context of Roslyn callback.

Short names can be used as parameter and variable names, namely `SyntaxTree tree`, `SemanticModel model`, `SyntaxNode node` and `CancellationToken cancel`.

### Method names

FIXME Avoid Get prefixes for method names. Save three characters when it only gets x.Foo.Bar.

### Unit tests

Unit tests for common C# and VB.NET rules should use two aliases `using CS = SonarAnalyzer.Rules.CSharp` and `using VB = SonarAnalyzer.Rules.VisualBasic`. Test method names should have `_CS` and `_VB` suffixes.

Unit tests for single language rule should not use alias nor language method suffix.

Variable name `sut` (System Under Test) is recommended in unit tests that really tests a single unit (contrary to our usual rule integration unit tests).

FIXME - Avoid names without meaning like `foo`, `bar`, `baz`. OR KISS?

Unit test method names:
- Underscore in UT names separates logical groups, not individual words.
- FIXME: what should the name pattern be? NEEDS DISCUSSION ([many patterns](https://dzone.com/articles/7-popular-unit-test-naming) and also [Microsoft convention](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices#naming-your-tests) - I'd go for MS convention)


## Multi-line statements

* Operators (`&&`, `||`, `and`, `or`, `+`, `:`, `?`, `??` and others) are placed at the beginning of a line.
    * Indented at the same level if the syntax at the beginning of the previous line is a sibling.
      ```csharp
      void Foo() =>
          A
          && B; // A and B are siblings => we don't indent
      ```
    * Indented one level further otherwise.  
      ```csharp
      return A
          && B; // "return" is the parent of A and B => we indent
      ```
* Dot before an invocation `.Method()` is placed at the beginning of a line.
* The comma separating arguments is placed at the end of a line.
* Method declaration parameters should be on the same line. If S103 is violated, parameters should be placed each on a separate line; the first parameter should be on the same line with the declaration; the other parameters should be aligned with the first parameter.
    ```csharp
    public void MethodWithManyParameters(int firstParameter,
                                         string secondParameter,
                                         Function<int, string, string> complexParameter);
    ```
* Long ternary operator statements should have `?` and `:` on separate lines, aligned with a left-most single indendation.
    ```csharp
    object.Property is SomeType something
    && something.AnotherProperty is OtherType other
    && other.Value == 42
        ? object.Parent.Value
        : object;
    ```
* Chained invocations and member accesses violating S103 can have a chain of properties on the first line. Every other `.Invocation()` or `.Member` should be on a separate line, aligned with a left-most single indendation.
    ```csharp
    object.Property.Children
        .Select(x => x.Something)
        .Where(x => x != null)
        .OrderBy(x => x.Rank)
        .ToArray()
        .Length;
    ```
  * Exception from this rule: Chains of assertions can have supporting properties, `.Should()` and assertion on the same line.
    ```csharp
    values.Should().HaveCount(2)
        .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
        .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
    ```
* Method invocation arguments should be placed on the same line only when they are few and simple. Otherwise, they should be placed on separate lines. The first argument should be on a separate line, aligned with a left-most single indendation.
    ```csharp
    object.MethodName(
        firstArgument,
        x => x.Bar(),
        thirdArgument.Property);
    ```
  * Exception from this rule: chained LINQ queries where the alignment of parameter expressions should be right-most.
    ```csharp
    someEnumerable.Where(x => x.Condition1
                              && x.Condition2);
    ```
  * Exception from this rule: Lambda parameter name and arrow token should be on the same line as the invocation.
    ```
    context.RegisterSyntaxNodeAction(c =>
        {
            // Action
        }
    ```
* When using an arrow property or an arrow method, the `=>` token must be on the same line as the declaration. Regarding the expression body:
  * for properties: it should be on the same line as the property declaration. It should be on the following line only when it is too long and would trigger S103.
  * for methods: it should be on the same line only for trivial cases: literal or simple identifier. Member access, indexer, invocation, and other complex structures should be on the following line.

## Code structure

### Principles

* When to factorize: two is a group, three is a crowd.
* Less is more.
* Rely on Roslyn Type inference to reduce used characters.

### Style

* Field and property initializations are done directly in the member declaration instead of in a constructor.
* `if`/`else if` and explicit `else` is used
  * when it helps to understand overall structure of the method,
  * especially when each branch ends with a `return` statement.
* Explicit `else` is not used after input validation.
* For multiple conditions before the core method logic:
  * chain conditions in the same `if` statement together with positive logic for best readability (i.e. `if (first && second) { DoSomething(); }`) 
  * when chained conditions cannot be used, use early returns
  * otherwise, use nested conditions
* Use positive logic.
* Use `is {}` and `is not null` as null-checks (instead of `!= null`).
* Var pattern `is var o` can be used only where variable declarations would require additional nesting.
* Var pattern `o is { P: var p }` can be used only where `o` can be `null` and `p` is used at least 3 times.
* Do not use `nullable`.
* Avoid single-use variables, unless they really bring readability value.
* Use file-scoped namespaces.
* Tested variable is on the left, like `iterated == searchParameter` (where `iterated` is the tested variable)
* If a string is multiline, use raw string literals, indented one tab-in from the declaration:
```
var raw = """
    hello
    my friend
	""";
```
* Always use multi-line initializers for collection and objects, e.g.:
```
var thingy = new Thingy
{
	x = "hello",
	y = 42,
}

var collection = new Dictionary<string, int>
{
	{ "hey" : 1 },
	{ "there": 42 },
}
```
* FIXME - align on how to use collection initializers int[] x = [ 1, 2, 3 ] or old style (see [slack discussion](https://sonarsource.slack.com/archives/C01H2B58DE1/p1697103918957899?thread_ts=1696951023.295859&cid=C01H2B58DE1))

### Unit Tests
* VerifierBuilder.AddSnippet should not be used to assert compliant/noncomplaint test cases. Move it to a TestCases file.


## Comments

* Code should contain as few comments as necessary in favor of well-named members and variables.
* Comments should generally be on separate lines.
* Comments on the same line with code are acceptable for short lines of code and short comments.
* Documentation comments for abstract methods and their implementations should be placed only on the abstract method, to avoid duplication. _When reading the implementation, the IDE offers the tooling to peek in the base class and read the method comment._
* Avoid using comments for "Arrange, Act, Assert" in UTs, unless the test is complex.
* Use single-line comments. Exception: `Internal /* for testing */ void Something()`.
* Prefer well-named members instead of documentation.
* When writing [xmldoc](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags) for methods, avoid adding superflous tags (e.g. members that have self-explanatory names).

## FIXME and ToDo

* `FIXME` should only be used in the code during development as a temporary reminder that there is still work to
be done here. A `FIXME` should never appear in a PR or be merged into master.

* `ToDo` can be used to mark part of the code that will need to be updated at a later time. It can be used to
track updates that should be done at some point, but that either cannot be done at that moment, or can be fixed later.
Ideally, a `ToDo` comment should be followed by an issue number (what needs to be done should be in the github issues).

## Regions

Generally, as we do not want to have classes that are too long, regions are not necessary and should be avoided.
It can still be used when and where it makes sense. For instance, when a class having a specific purpose is
implementing generic interfaces (such as `IComparable`, `IDisposable`), it can make sense to have regions 
for the implementation of these interfaces.

## Spacing

* Avoid spaces unless they bring clarity and help the reader understand logical groups. Prefer spaces over comments.

## Type definition

* If a class is a POCO data-container, use a record.
* Do not use primary constructors on normal classes and structs, use standard constructor + field/properties.

## ValueTuples

Do not use `ValueTuples` in production code. The usage in test projects is fine. `ValueTuples` are not supported in MsBuild 14 and while MsBuild 14 is not officially supported anymore, we still don't want to break it, if we can avoid it.

## VB.NET Specifics

Empty lines should be used between blocks, `Namespace`/`End Namespace` statements, `Class`/`End Class` statements
and regions to improve readability.

## Test scenarios files

For any C# rule `TheRule`, there should be at least two test scenarios files:
* `TheRule.cs`, targeting both .NET Framework and .NET Core and using the default version of C#
* `TheRule.Latest.cs`, targeting .NET Core only (via `# if NET`) and using the latest version of C#

More test scenarios files can be created as needed, for code fixes, top-level statements etc.

In the past, test scenarios were split by language (e.g. `TheRule.CSharpX.cs`). These files should be migrated, following a Clean as You Code approach.