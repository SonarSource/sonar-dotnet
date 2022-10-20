# Specifying Issues for the IssueLocationCollector  

Here's a summary and examples of the different patterns that can be used to mark part of the code as noncompliant when using the [IssueLocationCollector.](../analyzers/tests/SonarAnalyzer.UnitTest/TestFramework/IssueLocationCollector.cs)

These patterns must appear after a single line comment.

The supported comment tokens: `//` for C#, `'` for VB.NET and `<!--` for XML).

<br>

# Patterns 

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

# Tips

Note that most of the previous patterns can be used together when needed.

```csharp
    private void MyMethod() // Noncompliant@+1 ^4#7 [MyIssueId] {{Remove this unused private method}}
```
