using System;
using System.Collections.Generic;
using FluentAssertions;

Fruit a = null;
var y = a switch
{
    null => 1,
    not not {Prop.Count: 1 } => 0 // Noncompliant {{Simplify negation here.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^
};

class Fruit { public List<int> Prop; }

class FPRepro_5789
{
    // https://github.com/SonarSource/sonar-dotnet/issues/5789
    public void SomeMethod()
    {
        double from = 16;
        double to = 23;
        int x = 1;
        double y = 1;
        var dt = DateTime.Now;
        var sut = TimeSpan.MaxValue;
        if (sut.Ticks > 0)
            Convert.ToChar(x);
        else
            Convert.ToChar(y);
        if (sut.Ticks > 0)
            sut.Should().BeGreaterThan(TimeSpan.FromMilliseconds(from)).And.BeLessThan(TimeSpan.FromMilliseconds(to));
        else
            sut.Should().BeGreaterThan(TimeSpan.FromMilliseconds(to)).And.BeLessThan(TimeSpan.FromMilliseconds(from));

        if (sut.Ticks > 0) // Noncompliant
            sut.Should().BeGreaterThan(TimeSpan.FromMilliseconds(from)).And.BeLessThan(TimeSpan.FromMilliseconds(to));
        else
            sut.Should().BeGreaterThan(TimeSpan.FromMilliseconds(from)).And.BeLessThan(TimeSpan.FromMilliseconds(from));
    }
}
