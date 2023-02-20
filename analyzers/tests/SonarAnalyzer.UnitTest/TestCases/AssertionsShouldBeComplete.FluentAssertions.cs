using System;
using FluentAssertions;

public class Program
{
    public void Test()
    {
        var s = "Test";
        s.Should();            // Noncompliant {{Complete the assertion}}
        s.Should().Be("Test"); // Compliant
    }
}
