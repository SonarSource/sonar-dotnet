## Unit test comment syntax

The rule unit tests use source files with special annotations in code comments to specify noncompliant code.

These annotation patterns must appear after a single line comment (the supported comment tokens: `//` for C#, `'` for VB.NET and `<!--` for XML).

### `Noncompliant` primary location comment

Use `Noncompliant` to mark the current line as the primary location of an expected issue.

```cs
    private void MyMethod() { } // Noncompliant
```

### Using offsets

Using `@[+-][0-9]+` after a `Noncompliant` or `Secondary` comment will mark the expected location to be offset by the given number of lines.

```cs
    private void MyMethod() { } // Noncompliant@+2 - issue is actually expected 2 lines after this comment
```

### Checking the issue message

The message raised by the issue can be checked using the `{{expected message}}` pattern.

```cs
    private void MyMethod() { } // Noncompliant {{Remove this unused private method}}
```

### Checking the precise/exact location of an issue

Only one precise location or column location can be present at one time. Precise location is used by adding `^^^^` comment under the location where the issue is expected. The alternative column location pattern can be used by following the `Noncompliant` or `Secondary` comment with `^X#Y` where `X` is the expected start column and Y the length of the issue.

```cs
    private void MyMethod() { } // Noncompliant
//  ^^^^^^^

    private void MyMethod() { } // Noncompliant ^4#7
```

### `Secondary` location comment

`Secondary` is used to mark the expected line of a [secondary location](https://github.com/SonarSource/sonar-dotnet/blob/master/analyzers/src/SonarAnalyzer.Common/Common/SecondaryLocation.cs) and must be used together with a primary location.

```cs
    if (myCondition) // Noncompliant
    {
      var a = null; // Secondary
    }
```

### Multiple issues per line

To declare that multiple issues are expected, each issue must be assigned an `[ID]`. All secondary locations associated with an issue must have the same ID. Use offsets to define multiple precise locations on a single line.

```cs
    var a = null;    // Noncompliant [myId2]
    if (myCondition) // Noncompliant [myId1, myId3]
    {
      a = null;      // Secondary [myId1, myId2]
    }

    private void MyMethod(int i1, int i2) { }
    //                    ^^^^^^
    //                            ^^^^^^ @-1
```

### Compilation Errors

`Error` will mark the line as the location of a compilation error. This is useful to test code snippets that cannot be compiled, as it is usually the case inside an IDE/Editor. To increase comprehensibility, the error code as well as some comments can be specified. These are ignored by the verification process.

```csharp
    string x = 2; // Error [CS0029]
    string x = 2; // Error [CS0029] Cannot implicitly convert int to string
    string x == 2 // Error [CS1002, CS1525]
    // Error@+1 [CS0029] Cannot implicitly convert int to string
    string x = 2; 
```

### Combining multiple patterns

Note that most of the previous patterns can be used together when needed.

```cs
    private void MyMethod() { } // Noncompliant@+1 ^4#7 [MyIssueId] {{Remove this unused private method}}

    private void MyMethod(int i1, int i2) { }
    //                    ^^^^^^             {{Message for issue 1}}
    //                            ^^^^^^ @-1 {{Message for issue 2}}
```

The code comment syntax logic is implemented by the [`IssueLocationCollector`](https://github.com/SonarSource/sonar-dotnet/blob/master/analyzers/tests/SonarAnalyzer.TestFramework/Verification/IssueValidation/IssueLocationCollector.cs) class.