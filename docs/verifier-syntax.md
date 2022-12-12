## Unit test comment syntax

The rule unit tests use source files with special annotations in code comments to specifiy uncompliant code.

These annotation patterns must appear after a single line comment (the supported comment tokens: `//` for C#, `'` for VB.NET and `<!--` for XML).

### Simple `Noncompliant` comment

Will mark the current line as expecting the primary location of an issue.

```cs
    private void MyMethod() // Noncompliant
```

### `Secondary` location comment

Must be used together with a main primary location to mark the expected line of a secondary location.

```cs
    if (myCondition) // Noncompliant
    {
      var a = null; // Secondary
    }
```

### Using offsets

Using `@[+-][0-9]+` after a `Noncompliant` or `Secondary` comment will mark the expected location to be offset by the given number of lines.

```cs
    private void MyMethod() // Noncompliant@+2 - issue is actually expected 2 lines after this comment
```

### Checking the issue message

The message raised by the issue can be checked using the `{{expected message}}` pattern.

```cs
    private void MyMethod() // Noncompliant {{Remove this unused private method}}
```

### Checking the precise/exact location of an issue

Only one precise location or column location can be present at one time. Precise location is used by adding `^^^^` comment under the location where the issue is expected. The alternative column location pattern can be used by following the `Noncompliant` or `Secondary` comment with `^X#Y` where `X` is the expected start column and Y the length of the issue.

```cs
    private void MyMethod() // Noncompliant
//  ^^^^^^^

    private void MyMethod() // Noncompliant ^4#7
```

### Multiple issues per line

To declare that multiple issues are expected, each issue must be assigned an id. All secondary locations associated with an issue must have the same id. Note that it is not possible to have multiple precise/column locations on a single line.

```cs
    var a = null; // Noncompliant [myId2]
    if (myCondition) // Noncompliant [myId1, myId3]
    {
      a = null; // Secondary [myId1, myId2]
    }
```

Note that most of the previous patterns can be used together when needed.

```cs
    private void MyMethod() // Noncompliant@+1 ^4#7 [MyIssueId] {{Remove this unused private method}}

    private void MyMethod(int i2, int i2)
//                        ^^^^^^             {{Message for issue 1}}
//                                ^^^^^^ @-1 {{Message for issue 2}}
```