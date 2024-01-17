using FluentAssertions;
using NFluent;
using NSubstitute;
using System;

namespace AllFrameworksTests
{
    internal class Tests
    {
        public void FluentAssertions(string s)
        {
            s.Should(); // Noncompliant
        }

        public void NFluent()
        {
            Check.That(0); // Noncompliant
        }

        public void NSubstitute(IComparable comparable)
        {
            comparable.Received(); // Noncompliant
        }
    }
}
