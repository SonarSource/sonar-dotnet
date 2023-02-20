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
        s?.Should();             // Noncompliant
        s?[0].Should();          // Noncompliant
        s.Should().Be("Test");   // Compliant
        s.Should()?.Be("Test");  // Compliant
        s.Should()!?.Be("Test"); // Compliant
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
