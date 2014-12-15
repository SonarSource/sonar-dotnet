using System;
using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests
{
    [TestClass]
    public class MetricsTest
    {
        [TestMethod]
        public void Lines()
        {
            Lines("").Should().Be(1);
            Lines("\n").Should().Be(2);
            Lines("\r").Should().Be(2);
            Lines("\r\n").Should().Be(2);
            Lines("\n").Should().Be(2);
            Lines("\n\r").Should().Be(3);
            Lines("using System;\r\n/*hello\r\nworld*/").Should().Be(3);
        }

        private static int Lines(string text)
        {
            return MetricsFor(text).Lines();
        }

        [TestMethod]
        public void CommentsWithoutHeaders()
        {
            CommentsWithoutHeaders("").NonBlank.Should().BeEmpty();
            CommentsWithoutHeaders("").NoSonar.Should().BeEmpty();

            CommentsWithoutHeaders("#ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();
            CommentsWithoutHeaders("using System; #ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();

            CommentsWithoutHeaders("// foo").NonBlank.Should().BeEmpty();
            CommentsWithoutHeaders("#if DEBUG\nfoo\n#endif\n// foo").NonBlank.Should().BeEmpty();

            CommentsWithoutHeaders("using System; // l1").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; // l1\n// l2").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithoutHeaders("using System; /* l1 */").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithoutHeaders("using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithoutHeaders("using System; /// foo").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; /** foo */").NonBlank.Should().BeEquivalentTo(1);

            CommentsWithoutHeaders("using System; /** foo \n \n bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithoutHeaders("using System; /** foo \r \r bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithoutHeaders("using System; /** foo \r\n \r\n bar */").NonBlank.Should().BeEquivalentTo(1, 3);

            CommentsWithoutHeaders("using System; // NOSONAR").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; // ooNOSONARoo").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; // nosonar").NoSonar.Should().BeEmpty();
            CommentsWithoutHeaders("using System; // nOSonAr").NoSonar.Should().BeEmpty();

            CommentsWithoutHeaders("using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEmpty();

            CommentsWithoutHeaders("using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEmpty();
        }

        private FileComments CommentsWithoutHeaders(string text)
        {
            return MetricsFor(text).Comments(true);
        }

        [TestMethod]
        public void CommentsWitHeaders()
        {
            CommentsWithHeaders("").NonBlank.Should().BeEmpty();
            CommentsWithHeaders("").NoSonar.Should().BeEmpty();

            CommentsWithHeaders("#ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();
            CommentsWithHeaders("using System; #ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEmpty();

            CommentsWithHeaders("// foo").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders("#if DEBUG\nfoo\n#endif\n// foo").NonBlank.Should().BeEquivalentTo(4);

            CommentsWithHeaders("using System; // l1").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; // l1\n// l2").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithHeaders("using System; /* l1 */").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithHeaders("using System; /* l1 \n l2 */").NonBlank.Should().BeEquivalentTo(1, 2);
            CommentsWithHeaders("using System; /// foo").NonBlank.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; /** foo */").NonBlank.Should().BeEquivalentTo(1);

            CommentsWithHeaders("using System; /** foo \n \n bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithHeaders("using System; /** foo \r \r bar */").NonBlank.Should().BeEquivalentTo(1, 3);
            CommentsWithHeaders("using System; /** foo \r\n \r\n bar */").NonBlank.Should().BeEquivalentTo(1, 3);

            CommentsWithHeaders("using System; // NOSONAR").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; // ooNOSONARoo").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; // nosonar").NoSonar.Should().BeEmpty();
            CommentsWithHeaders("using System; // nOSonAr").NoSonar.Should().BeEmpty();

            CommentsWithHeaders("using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEmpty();

            CommentsWithHeaders("using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEmpty();
        }

        private FileComments CommentsWithHeaders(string text)
        {
            return MetricsFor(text).Comments(false);
        }

        [TestMethod]
        public void Classes()
        {
            Classes("").Should().Be(0);
            Classes("class MyClass {}").Should().Be(1);
            Classes("class MyClass1 {} namespace MyNamespace { class MyClass2 {} }").Should().Be(2);
        }

        private static int Classes(string text)
        {
            return MetricsFor(text).Classes();
        }

        [TestMethod]
        public void Accessors()
        {
            Accessors("").Should().Be(0);
            Accessors("class MyClass { public int MyField; public MyClass() {} public int MyMethod() { return 42; } }").Should().Be(0);
            Accessors("class MyClass { public int MyProperty { get; } }").Should().Be(1);
            Accessors("class MyClass { public int MyProperty { get; set; } }").Should().Be(2);
            Accessors("class MyClass { public int MyProperty { get { return 0; } set { } } }").Should().Be(2);
            Accessors("class MyClass { public event EventHandler OnSomething { add { } remove {} }").Should().Be(2);
        }

        private static int Accessors(string text)
        {
            return MetricsFor(text).Accessors();
        }

        [TestMethod]
        public void Statements()
        {
            Statements("").Should().Be(0);
            Statements("class MyClass {}").Should().Be(0);
            Statements("class MyClass { void MyMethod() {} }").Should().Be(0);
            Statements("class MyClass { void MyMethod() { {} } }").Should().Be(0);
            Statements("class MyClass { int MyMethod() { return 0; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { int l = 42; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { Console.WriteLine(); } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { ; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { foo: ; } }").Should().Be(2);
            Statements("class MyClass { void MyMethod() { goto foo; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { break; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { continue; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { throw; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { yield return 42; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { yield break; } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { while (false) {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { do {} while (false); } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { for (;;) {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { foreach (var e in c) {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { using (var e = new MyClass()) {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { fixed (int* p = &pt.x) {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { checked {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { unchecked {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { unsafe {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { if (false) {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { switch (v) { case 0: ; } } }").Should().Be(2);
            Statements("class MyClass { void MyMethod() { try {} catch {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { try {} finally {} } }").Should().Be(1);
            Statements("class MyClass { void MyMethod() { try {} catch {} finally {} } }").Should().Be(1);
            Statements("class MyClass { int MyMethod() { int a = 42; Console.WriteLine(a); return a; } }").Should().Be(3);
        }

        private static int Statements(string text)
        {
            return MetricsFor(text).Statements();
        }

        private static Metrics MetricsFor(string text)
        {
            return new Metrics(CSharpSyntaxTree.ParseText(text));
        }
    }
}
