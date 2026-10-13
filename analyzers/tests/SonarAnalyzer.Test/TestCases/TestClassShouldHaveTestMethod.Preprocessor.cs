using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

[TestClass]
public class NetTests
{
#if NET
    [TestMethod]
    public void Test() { }
#endif
}

[TestClass]
public class DebugTests
{
#if DEBUG
    [TestMethod]
    public void Test() { }
#endif
}

[TestClass]
public class CustomTests
{
#if CUSTOM
    [TestMethod]
    public void Test() { }
#endif
}

[TestClass]
public class ElifTests
{
#if false
#elif DEBUG
    [TestMethod]
    public void Test() { }
#endif
}

[TestClass]
public class ElseTests
{
#if DEBUG
#else
    [TestMethod]
    public void Test() { }
#endif
}

[TestClass]
public class NestedTests
{
#if DEBUG
#if CUSTOM
    [TestMethod]
    public void Test() { }
#endif
#endif
}

[TestClass]
public class CompoundConditionTests
{
#if !DEBUG && !CUSTOM
    [TestMethod]
    public void Test() { }
#endif
}

[TestClass]
public class ConditionalAttributeTests
{
#if DEBUG
    [TestMethod]
#endif
    public void Test() { }
}

public partial class PartialTests
{
#if DEBUG
    [TestMethod]
    public void Test() { }
#endif
}

public abstract class BaseTests
{
#if DEBUG
    [TestMethod]
    public void Test() { }
#endif
}

public abstract class IntermediateTests : BaseTests { }

[TestClass]
public class InheritedTests : IntermediateTests { }

[TestFixture]
public class NUnitTests
{
#if DEBUG
    [Test]
    public void Test() { }
#endif
}

[TestClass]
public class ConditionalBodyTests
{
    [TestMethod]
    public void Test()
    {
#if DEBUG
        System.Console.WriteLine("Debug");
#endif
    }
}

#if DEBUG
[TestClass]
public class ConditionalClassTests
{
    [TestMethod]
    public void Test() { }
}
#endif

[TestClass]
public class EmptyConditionalBlock // Noncompliant
{
#if DEBUG
#endif
}

[TestClass]
public partial class EmptyPartialTests { } // Noncompliant
public partial class EmptyPartialTests { } // Noncompliant

[TestClass]
public class ActiveCodeWithoutTests // Noncompliant
{
#if true
    public void Helper() { }
#endif
}

[TestClass]
public class RegionWithoutTests // Noncompliant
{
#region Helpers
    public void Helper() { }
#endregion
}

[TestClass]
public class OtherDirectivesWithoutTests // Noncompliant
{
#pragma warning disable CS0168
#line 1
    public void Helper() { }
#line default
#pragma warning restore CS0168
}

#if false
public class UnrelatedInactiveCode { }
#endif
[TestClass]
public class EmptyClassAfterInactiveCode // Noncompliant
{
}

[TestClass]
public class EmptyClassBeforeInactiveCode // Noncompliant
{
}
#if false
public class MoreUnrelatedInactiveCode { }
#endif

[TestClass]
public class ConditionalHelperBody // Noncompliant
{
    public void Helper()
    {
#if DEBUG
        System.Console.WriteLine("Debug");
#endif
    }
}

[TestClass]
public class ConditionalHelperExpression // Noncompliant
{
    public int Helper() =>
#if DEBUG
        1;
#else
        2;
#endif
}

[TestClass]
public class ConditionalProperty // Noncompliant
{
    public int Value
    {
        get
        {
#if DEBUG
            return 1;
#else
            return 2;
#endif
        }
    }
}

[TestClass]
public class ConditionalFieldInitializer // Noncompliant
{
    private int value =
#if DEBUG
        1;
#else
        2;
#endif
}

[TestClass]
public class ConditionalAttributeArgument // Noncompliant
{
    [System.Obsolete(
#if DEBUG
        "Debug"
#else
        "Release"
#endif
    )]
    public void Helper() { }
}

[TestClass]
public class ConditionalParameterDefault // Noncompliant
{
    public void Helper(int value =
#if DEBUG
        1
#else
        2
#endif
    ) { }
}

[TestClass]
public class ConditionalConstructorInitializer // Noncompliant
{
    public ConditionalConstructorInitializer() : this(
#if DEBUG
        1
#else
        2
#endif
    ) { }

    public ConditionalConstructorInitializer(int value) { }
}

[TestClass]
public class ConditionalTypeParameterConstraint // Noncompliant
{
    public void Helper<T>() where T :
#if DEBUG
        System.IDisposable
#else
        System.IComparable
#endif
    { }
}

[TestClass]
public class ConditionalWholeBody // Noncompliant
{
    public void Helper()
#if DEBUG
    { System.Console.WriteLine("Debug"); }
#else
    { }
#endif
}

[TestClass]
public class ConditionalReturnType // Noncompliant
{
    public
#if DEBUG
        int
#else
        long
#endif
        Helper() => 0;
}

[TestClass]
[System.Obsolete(
#if DEBUG
    "Debug"
#else
    "Release"
#endif
)]
public class ConditionalClassAttributeArgument // Noncompliant
{
}

[TestClass]
public class OuterClassWithNestedTests // Noncompliant
{
    [TestClass]
    public class NestedTests
    {
#if DEBUG
        [TestMethod]
        public void Test() { }
#endif
    }
}

public abstract class BaseWithConditionalHelper
{
    public void Helper()
    {
#if DEBUG
        System.Console.WriteLine("Debug");
#endif
    }
}

[TestClass]
public class InheritedConditionalHelper : BaseWithConditionalHelper { } // Noncompliant

[TestClass]
public partial class PartialConditionalHelper { } // Noncompliant

public partial class PartialConditionalHelper // Noncompliant
{
    public void Helper()
    {
#if DEBUG
        System.Console.WriteLine("Debug");
#endif
    }
}

[TestClass]
public class ConditionalTestsBeforeNestedType
{
#if DEBUG
    [TestMethod]
    public void Test() { }
#endif
    public class Nested { }
}

[TestClass]
public class ConditionalTestsBeforeField
{
#if DEBUG
    [TestMethod]
    public void Test() { }
#endif
    private int value;
}

[TestClass]
public class ConditionalAttributeAfterActiveAttribute
{
    [System.Obsolete]
#if DEBUG
    [TestMethod]
#endif
    public void Test() { }
}

[TestClass]
public class ConditionalAttributeInList
{
    [System.Obsolete,
#if DEBUG
    TestMethod,
#endif
    System.CLSCompliant(false)]
    public void Test() { }
}

[TestClass]
public class ConditionalTestsWithAlternativeHelper
{
#if DEBUG
    [TestMethod]
    public void Test() { }
#else
    public void Helper() { }
#endif
}

[TestClass]
public class ConditionalFieldAttributeAfterActiveAttribute
{
    [System.Obsolete]
#if DEBUG
    [System.CLSCompliant(true)]
#else
    [System.CLSCompliant(false)]
#endif
    public int value;
}

[TestClass]
public class ConditionalAccessor // Noncompliant
{
    public int Value
    {
        get;
#if DEBUG
        set;
#endif
    }
}

[TestClass]
public class ConditionalBodyInsideActiveRegion // Noncompliant
{
#if true
    public void Helper()
    {
#if DEBUG
        System.Console.WriteLine("Debug");
#endif
    }
#endif
}

#if true
[TestClass]
public class EmptyClassInsideConditionalRegion // Noncompliant
{
}
#else
public class UnrelatedAlternativeClass { }
#endif

#if true
public class ActiveSibling { }

[TestClass]
public class EmptyClassInsideFileConditionalRegion // Noncompliant
{
#region Helpers
#endregion
}
#else
public class UnrelatedAlternativeFile { }
#endif

#if true
[TestClass]
public class HiddenTestsInsideFileConditionalRegion
{
#if DEBUG
    [TestMethod]
    public void Test() { }
#endif
}
#else
public class AnotherUnrelatedAlternativeFile { }
#endif
