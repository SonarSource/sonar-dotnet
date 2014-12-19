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
        public void LinesOfCode()
        {
            LinesOfCode("").Should().BeEquivalentTo();
            LinesOfCode("/* ... */\n").Should().BeEquivalentTo();
            LinesOfCode("namespace { }").Should().BeEquivalentTo(1);
            LinesOfCode("namespace \n { \n }").Should().BeEquivalentTo(1, 2, 3);
            LinesOfCode("public class MyClass { public MyClass() { Console.WriteLine(@\"line1 \n line2 \n line3 \n line 4\"); } }").Should().BeEquivalentTo(1, 2, 3, 4);
        }

        private static IImmutableSet<int> LinesOfCode(string text)
        {
            return MetricsFor(text).LinesOfCode();
        }

        [TestMethod]
        public void CommentsWithoutHeaders()
        {
            CommentsWithoutHeaders("").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders("").NoSonar.Should().BeEquivalentTo();

            CommentsWithoutHeaders("#ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders("using System; #ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders("// foo").NonBlank.Should().BeEquivalentTo();
            CommentsWithoutHeaders("#if DEBUG\nfoo\n#endif\n// foo").NonBlank.Should().BeEquivalentTo();

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
            CommentsWithoutHeaders("using System; // nosonar").NoSonar.Should().BeEquivalentTo();
            CommentsWithoutHeaders("using System; // nOSonAr").NoSonar.Should().BeEquivalentTo();

            CommentsWithoutHeaders("using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEquivalentTo();

            CommentsWithoutHeaders("using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithoutHeaders("using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEquivalentTo();
        }

        private FileComments CommentsWithoutHeaders(string text)
        {
            return MetricsFor(text).Comments(true);
        }

        [TestMethod]
        public void CommentsWitHeaders()
        {
            CommentsWithHeaders("").NonBlank.Should().BeEquivalentTo();
            CommentsWithHeaders("").NoSonar.Should().BeEquivalentTo();

            CommentsWithHeaders("#ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();
            CommentsWithHeaders("using System; #ifdef DEBUG\nfoo\n#endif").NonBlank.Should().BeEquivalentTo();

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
            CommentsWithHeaders("using System; // nosonar").NoSonar.Should().BeEquivalentTo();
            CommentsWithHeaders("using System; // nOSonAr").NoSonar.Should().BeEquivalentTo();

            CommentsWithHeaders("using System; /* NOSONAR */ /* foo*/").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; /* NOSONAR */ /* foo */").NonBlank.Should().BeEquivalentTo();

            CommentsWithHeaders("using System; /* foo*/ /* NOSONAR */").NoSonar.Should().BeEquivalentTo(1);
            CommentsWithHeaders("using System; /* foo*/ /* NOSONAR */").NonBlank.Should().BeEquivalentTo();
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

        [TestMethod]
        public void Functions()
        {
            Functions("").Should().Be(0);
            Functions("class MyClass { }").Should().Be(0);
            Functions("abstract class MyClass { public abstract void MyMethod1(); }").Should().Be(0);
            Functions("class MyClass { public int MyProperty1 { get; set; } }").Should().Be(0);
            Functions("class MyClass { static MyClass() { } }").Should().Be(1);
            Functions("class MyClass { public MyClass() { } }").Should().Be(1);
            Functions("class MyClass { ~MyClass() { } }").Should().Be(1);
            Functions("class MyClass { public void MyMethod2() { } }").Should().Be(1);
            Functions("class MyClass { public static MyClass operator +(MyClass a) { return a; } }").Should().Be(1);
            Functions("class MyClass { public int MyProperty2 { get { return 0; } } }").Should().Be(1);
            Functions("class MyClass { public int MyProperty3 { set { } } }").Should().Be(1);
            Functions("class MyClass { public int MyProperty4 { get { return 0; } set { } } }").Should().Be(2);
            Functions("class MyClass { public event EventHandler OnSomething { add { } remove {} } }").Should().Be(2);
        }

        private static int Functions(string text)
        {
            return MetricsFor(text).Functions();
        }

        [TestMethod]
        public void PublicApi()
        {
            PublicApi("").Should().Be(0);
            PublicApi("class MyClass { }").Should().Be(0);
            PublicApi("public class MyClass { }").Should().Be(1);
            PublicApi("namespace MyNS { }").Should().Be(0);
            PublicApi("public class MyClass { }").Should().Be(1);
            PublicApi("public class MyClass { public void MyMethod() { } }").Should().Be(2);
            PublicApi("private class MyClass { public void MyMethod() { } }").Should().Be(0);
            PublicApi("public class MyClass { public int MyField; }").Should().Be(2);
            PublicApi("public interface MyInterface { public class MyClass { } }").Should().Be(2);
            PublicApi("namespace MyNS { public class MyClass { } }").Should().Be(1);
            PublicApi("public class MyClass { public event EventHandler OnSomething(); }").Should().Be(2);
            PublicApi("public class MyClass { public delegate void Foo(); }").Should().Be(2);
            PublicApi("public class MyClass { public static MyClass operator +(MyClass a) { return a; } }").Should().Be(2);
            PublicApi("public class MyClass { public class MyClass2 { public int MyField; } }").Should().Be(3);
            PublicApi("public enum MyEnum { MyValue1, MyValue2 }").Should().Be(1);
            PublicApi("public struct MyStruct { public int MyField; }").Should().Be(2);
            PublicApi("public class MyClass { public MyClass this[int i] { return null; } }").Should().Be(2);
            PublicApi("public class MyClass { public int MyProperty { get; set; } }").Should().Be(2);
            PublicApi("public class MyClass { void MyMethod() { } }").Should().Be(1);
            PublicApi("public class MyClass { private void MyMethod() { } }").Should().Be(1);
            PublicApi("public class MyClass { protected MyMethod() { } }").Should().Be(1);
        }

        private static int PublicApi(string text)
        {
            return MetricsFor(text).PublicApi();
        }

        [TestMethod]
        public void PublicUndocumentedApi()
        {
            PublicUndocumentedApi("").Should().Be(0);
            PublicUndocumentedApi("class MyClass { }").Should().Be(0);
            PublicUndocumentedApi("public class MyClass { }").Should().Be(1);
            PublicUndocumentedApi("/* ... */ public class MyClass { }").Should().Be(0);
            PublicUndocumentedApi("// ... \n public class MyClass { }").Should().Be(0);
            PublicUndocumentedApi("public class MyClass { \n public int MyField; }").Should().Be(2);
            PublicUndocumentedApi("public class MyClass { \n /* ... */ public int MyField; }").Should().Be(1);
            PublicUndocumentedApi("/// ... \n public class MyClass { \n /* ... */ public int MyField; }").Should().Be(0);
        }

        private static int PublicUndocumentedApi(string text)
        {
            return MetricsFor(text).PublicUndocumentedApi();
        }

        [TestMethod]
        public void Complexity()
        {
            Complexity("").Should().Be(0);
            Complexity("class MyClass { }").Should().Be(0);
            Complexity("abstract class MyClass { abstract void MyMethod(); }").Should().Be(0);
            Complexity("class MyClass { void MyMethod() { } }").Should().Be(1);
            Complexity("class MyClass { void MyMethod() { return; } }").Should().Be(1);
            Complexity("class MyClass { void MyMethod() { return; return; } }").Should().Be(2);
            Complexity("class MyClass { void MyMethod() { { return; } } }").Should().Be(2);
            Complexity("class MyClass { void MyMethod() { if (false) { } } }").Should().Be(2);
            Complexity("class MyClass { void MyMethod() { if (false) { } else { } } }").Should().Be(2);
            Complexity("class MyClass { void MyMethod(int p) { switch (p) { default: break; } }").Should().Be(2);
            Complexity("class MyClass { void MyMethod(int p) { switch (p) { case 0: break; default: break; } }").Should().Be(3);
            Complexity("class MyClass { void MyMethod(int p) { foo: ; }").Should().Be(2);
            Complexity("class MyClass { void MyMethod(int p) { do { } while (false); }").Should().Be(2);
            Complexity("class MyClass { void MyMethod(int p) { for (;;) { } }").Should().Be(2);
            Complexity("class MyClass { void MyMethod(List<int> p) { foreach (var i in p) { } }").Should().Be(2);
            Complexity("class MyClass { void MyMethod(int p) { var a = false; }").Should().Be(1);
            Complexity("class MyClass { void MyMethod(int p) { var a = false && false; }").Should().Be(2);
            Complexity("class MyClass { void MyMethod(int p) { var a = false || true; }").Should().Be(2);
            Complexity("class MyClass { int MyProperty { get; set; } }").Should().Be(0);
            Complexity("class MyClass { int MyProperty { get {} set {} } }").Should().Be(2);
            Complexity("class MyClass { public MyClass() { } }").Should().Be(1);
            Complexity("class MyClass { ~MyClass() { } }").Should().Be(1);
            Complexity("class MyClass { public static MyClass operator +(MyClass a) { return a; } }").Should().Be(1);
            Complexity("class MyClass { public event EventHandler OnSomething { add { } remove {} } }").Should().Be(2);
        }

        private static int Complexity(string text)
        {
            return MetricsFor(text).Complexity();
        }

        [TestMethod]
        public void FunctionComplexityDistribution()
        {
            FunctionComplexityDistribution("").Ranges.Should().BeEquivalentTo(1, 2, 4, 6, 8, 10, 12);

            FunctionComplexityDistribution("").Values.Should().BeEquivalentTo(0, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution("class MyClass { void M1() { } }").Values.Should().BeEquivalentTo(1, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution("class MyClass { void M1() { } void M2() { } }").Values.Should().BeEquivalentTo(2, 0, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution("class MyClass { void M1() { if (false) { } } }").Values.Should().BeEquivalentTo(0, 1, 0, 0, 0, 0, 0);
            FunctionComplexityDistribution("class MyClass { void M1() { if (false) { } if (false) { } if (false) { } } }").Values.Should().BeEquivalentTo(0, 0, 1, 0, 0, 0, 0);
        }

        private static Distribution FunctionComplexityDistribution(string text)
        {
            return MetricsFor(text).FunctionComplexityDistribution();
        }

        [TestMethod]
        public void CopyPasteTokens()
        {
            CopyPasteTokens("").Should().BeEquivalentTo();
            CopyPasteTokens("using System;").Should().BeEquivalentTo();

            CopyPasteTokens("class MyClass { /* ... */ \n string MyField = \"hehe\"; }")
                .Should()
                .BeEquivalentTo(
                    Tuple.Create("class", 1), Tuple.Create("MyClass", 1), Tuple.Create("{", 1),
                    Tuple.Create("string", 2), Tuple.Create("MyField", 2), Tuple.Create("=", 2),
                    Tuple.Create("\"\"", 2), Tuple.Create(";", 2), Tuple.Create("}", 2));
        }

        private static ImmutableArray<Tuple<string, int>> CopyPasteTokens(string text)
        {
            return MetricsFor(text).CopyPasteTokens();
        }

        private static Metrics MetricsFor(string text)
        {
            return new Metrics(CSharpSyntaxTree.ParseText(text));
        }
    }
}
