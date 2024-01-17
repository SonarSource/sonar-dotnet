using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace ImplementsTestBuilder
{
    public class CustomTestAttribute : Attribute, ITestBuilder
    {
        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite) => throw new NotImplementedException();
    }

    public class DerivedCustomTestAttribute : CustomTestAttribute { }

    public interface IWrongInterface { }

    public class FakeAttribute : Attribute, IWrongInterface { }

    [TestFixture]
    public class Fixture
    {
        [CustomTest]
        public void Foo() { }
    }

    [TestFixture]
    public class Fixture2
    {
        [DerivedCustomTest]
        public void Bar() { }
    }

    [TestFixture]
    public class Fixture3 // Noncompliant
//               ^^^^^^^^
    {
        [FakeAttribute]
        public void Baz() { }
    }

    [TestFixture]
    public class A // Noncompliant, FP - In NUnit this scenario is valid. See: https://github.com/SonarSource/sonar-dotnet/issues/5732
    {
        public class Nested : A
        {
            [Test]
            public void Test()
            {
            }
        }
    }
}
