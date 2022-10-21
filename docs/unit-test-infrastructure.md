# Unit Test Infrastructure 

In order to test the functionality of an analyzer, an instance of `VerifierBuilder` should be created and then an invocation of `.Verify()` or `.VerifyCodeFix()` should be made. 

In order to instantiate a `VerifiedBuilder`, a generic argument is passed that has to implement the `DiagnosticAnalyzer` base class.

```csharp
    public class MyAnalyzer : DiagnosticAnalyzer
    {
        // analysis work
    }

    [TestClass]
    public class Testbed
    {
        private VerifierBuilder myBuilder = new VerifierBuilder<MyAnalyzer>();
    }
```

In order to configure the `VerifiedBuilder` instance, some "add/with" methods are exposed.

`VerifiedBuilder` functions as an immutable record, so every "add/with" call returns a new instance of the class with the required mutations applied.

When `.Verify()` or `.VerifyCodeFix()` is called, a compilation is created based on the configuration of the instance and then the expected issues and errors are compared with the actual diagnostics produced.

### AddPaths

Adds some testcases (source files), to the compilation. These files are marked by specific [patterns](#patterns).

```csharp
    myBuilder = myBuilder.AddPaths("Testcase.cs");
```

### AddReferences

Adds some metadata references, which might be required for the compilation.

```csharp
    myBuilder = myBuilder.AddReferences(MetadataReferenceFacade.SystemData);
```

### WithOptions

Adds some options, for example `LanguageVersion`, `SourceCodeKind` to the `VerifiedBuilder` configuration.

Usually these are bundled all together based on the language version targeted, using the `ParseOptionsHelper` class.

```csharp
    myBuilder = myBuilder.WithOptions(ParseOptionsHelper.FromCSharp10);
```

### WithCodeFix, WithCodeFixedPaths

Some rules can provide a code fix suggestion, which can be provided to the `VerifiedBuilder` instance.

The code fix is a generic argument that should implement `SonarCodeFix`.

```csharp
    private class  MyCodeFix: SonarCodeFix 
    {
        // code fix work
    }

    myBuilder = myBuilder.WithCodeFix<MyCodeFix>();
```

The "corrected" code should also be supplied, in order to compare the expected fixed code with the actual compilation results.

```csharp
    myBuilder = myBuilder.WithCodeFixedPaths("Testcase.Fixed.cs")
```


### Full Example

```csharp
    public class MyAnalyzer : DiagnosticAnalyzer
    {
        // analysis work
    }

    public class MyCodeFix : SonarCodeFix
    {
        // code fix work
    }

    public class Testbed
    {
        private VerifierBuilder myBuilder = new VerifierBuilder<MyAnalyzer>()
            .AddPaths("Testcase.cs")
            .AddReferences(MetadataReferenceFacade.SystemData);

        [TestMethod]
        public void MyAnalyzerTest_FromCSharp10() =>
            myBuilder.WithOptions(ParseOptionsHelper.FromCSharp10).Verify();


        [TestMethod]
        public void MyAnalyzerTest_FromCSharp11() =>
            myBuilder.WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

        [TestMethod]
        public void MyAnalyzerTest_FromCSharp11_CodeFix() =>
            myBuilder
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .WithCodeFix<MyCodeFix>()
                .WithCodeFixedPaths("Testcase.Fixed.cs")
                .VerifyCodeFix();
    }
```

# Patterns 

Here's a summary and examples of the different patterns that can be used to mark part of the code as noncompliant.

More detail can be found on the [IssueLocationCollector.cs](../analyzers/tests/SonarAnalyzer.UnitTest/TestFramework/IssueLocationCollector.cs)

These patterns must appear after a single line comment.

The supported comment tokens: `//` for C#, `'` for VB.NET and `<!--` for XML).

<br>

### Simple 'Noncompliant' comment

Will mark the current line as expecting the primary location of an issue.

```csharp
     private void MyMethod() // Noncompliant
```

<br>

### 'Secondary' location comment 

Must be used together with a main primary location to mark the expected line of a secondary location.

```csharp
     if (myCondition) // Noncompliant
     {
       var a = null; // Secondary
     }
```     

<br>

### Using offsets

Using @[+-][0-9]+ after a 'Noncompliant' or 'Secondary' comment will mark the expected location to be offset by the given number of lines.


```csharp
    private void MyMethod() // Noncompliant@+2 - issue is actually expected 2 lines after this comment
```

<br>

### Checking the issue message

The message raised by the issue can be checked using the {{expected message}} pattern.


```csharp
    private void MyMethod() // Noncompliant {{Remove this unused private method}}
```

<br>

### Checking the precise/exact location of an issue 

Only one precise location or column location can be present at one time. 

Precise location is used by adding '^^^^' comment under the location where the issue is expected.

```csharp
    private void MyMethod() // Noncompliant
//  ^^^^^^^
```

The alternative column location pattern can be used by following the 'Noncompliant' or 'Secondary' comment with '^X#Y' where 'X' is the expected start column and Y the length of the issue.

```csharp
    private void MyMethod() // Noncompliant ^4#7
```

<br>

### Multiple issues per line

To declare that multiple issues are expected, each issue must be assigned an id. 
All secondary locations associated with an issue must have the same id. 

Note that it is not possible to have multiple precise/column locations on a single line.

```csharp
    var a = null; // Noncompliant [myId2]
    if (myCondition) // Noncompliant [myId1, myId3]
    {
      a = null; // Secondary [myId1, myId2]
    }
```

<br>

### Compilation Errors

Will mark the line as the location of a compilation error. The error code can also be specified if needed, as well as some comments for readability.

```csharp
    string x = 2; // Error 
    string x = 2; // Error [CS0029]
    string x = 2; // Error [CS0029] - cannot implicitly convert int to string
```

<br>

# Tips

Note that most of the previous patterns can be used together when needed.

```csharp
    private void MyMethod() // Noncompliant@+1 ^4#7 [MyIssueId] {{Remove this unused private method}}
```
