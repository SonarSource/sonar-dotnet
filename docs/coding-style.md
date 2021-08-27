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
1. Private nested classes

Furthermore, each of these categories should be ordered from higher to lower accessibility level (public, internal, protected, private).

Static fields and properties should be placed before instance ones. 

Static methods are preferred to be after instance methods.

## Naming conventions

Single variable lambdas should use `x` as the variable name (based on lambda calculus λx). Multi variable lambdas should use descriptive names, where `x` can be used for the main iterated item like `(x, index) => ...`. Name `c` can be used for context of Roslyn callback.

Unit tests for common C# and VB.NET rules should use two aliases `using CS = SonarAnalyzer.Rules.CSharp` and `using VB = SonarAnalyzer.Rules.VisualBasic`. Test method names should have `_CS` and `_VB` suffixes.

Unit tests for single language rule should not use alias nor language method suffix.

Variable name `sut` (System Under Test) is recommended in unit tests that really tests a single unit (contrary to our usual rule integration unit tests).

## Multi-line statements

* Operators (&&, ||, +, :, ?, ?? and others) are placed at the beginning of a line.
* Dot inside an invocation `.Method()` is placed at the beginning of a line.
* The comma separating arguments is placed at the end of a line.
* Method declaration parameters should be on the same line. If S103 is violated, parameters should be placed each on a separate line; the first parameter should be on the same line with the declaration; the other parameters should be aligned with the first parameter.
```
public void MethodWithManyParameters(int firstParameter,
                                     string secondParameter,
                                     Function<int, string, string> complexParameter);
```
* Method invocation arguments should be placed on the same line only when they are few and simple. Otherwise, they should be placed on separate lines. The first argument should be on a separate line, aligned with a left-most single indendation.
```
object.MethodName(
  firstArgument,
  x => x.Bar(),
  thirdArgument.Property);
```
  * Exception from this rule: chained LINQ queries where the alignment of parameter expressions should be right-most.
```
someEnumerable.Select(x => x.Foo).Where(x => x.Condition1
                                             && x.Condition2);
```
* Expression body of arrow property should be on the same line, as the property declaration. It should be on next line only when S103 is violated.
* Expression body of method should be on the same line only for trivial cases: literal or simple identifier. Member access, indexer, invocation and other complex structures should be on the next line.

## Code structure

* Field and property initializations are done directly in the member declaration instead of in a constructor.
* `if`/`else if` and explicit `else` is used
  * when it helps to understand overall structure of the method,
  * especially when each branch ends with a `return` statement.
* Explicit `else` is not used after input validation.

## Comments

* Code should contain as few comments as necessary in favor of well-named members and variables.
* Comments should generally be on separate lines.
* Comments on the same line with code are acceptable for short lines of code and short comments.
* Documentation comments for abstract methods and their implementations should be placed only on the abstract method, to avoid duplication. _When reading the implementation, the IDE offers the tooling to peek in the base class and read the method comment._

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

## VB.NET Specifics

Empty lines should be used between blocks, `Namespace`/`End Namespace` statements, `Class`/`End Class` statements
and regions to improve readability.
