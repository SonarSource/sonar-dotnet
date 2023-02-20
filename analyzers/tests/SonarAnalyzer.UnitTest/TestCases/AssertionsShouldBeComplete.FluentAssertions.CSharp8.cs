#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Primitives;

public class Program
{
    public void StringAssertions()
    {
        var s = "Test";
        s.Should();              // Noncompliant (no "ambiguous calls" compiler error as in C# 7)
        s?.Should();             // Noncompliant
        s?[0].Should();          // Noncompliant
        s.Should().Be("Test");   // Compliant
        s.Should()?.Be("Test");  // Compliant
        s.Should()!?.Be("Test"); // Compliant
    }

    public void CollectionAssertions()
    {
        var collection = new[] { "Test", "Test" };
        collection.Should(); // Noncompliant (no "ambiguous calls" compiler error as in C# 7)
    }

    public void DictAssertions()
    {
        var dict = new Dictionary<string, object>();
        dict["A"]?.Should();   // Noncompliant
        //         ^^^^^^
        dict?["A"]?.Should();  // Noncompliant
        //          ^^^^^^
        dict?["A"]!.Should();  // Noncompliant
        //          ^^^^^^
    }
}
