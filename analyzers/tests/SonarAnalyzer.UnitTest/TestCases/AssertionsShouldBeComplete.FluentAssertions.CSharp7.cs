using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Primitives;

public class Program
{
    public void StringAssertions()
    {
        var s = "Test";
        s.Should();                   // Noncompliant {{Complete the assertion}}
        //^^^^^^
        s[0].Should();                // Error [CS0121] ambiguous calls
                                      // Noncompliant@-1

        s.Should().Be("Test");        // Compliant
    }

    public void CollectionAssertions()
    {
        var collection = new[] { "Test", "Test" };
        collection.Should();                               // Error [CS0121] ambiguous calls
                                                           // Noncompliant@-1
        collection.Should<string>();                       // Noncompliant

        collection.Should<string>().Equal("Test", "Test"); // Compliant
    }

    public void DictionaryAssertions()
    {
        var dict = new Dictionary<string, object>();
        dict["A"].Should();    // Noncompliant
    }

    public StringAssertions ReturnedByReturn()
    {
        var s = "Test";
        return s.Should();     // Compliant
    }

    public StringAssertions ReturnedByArrow(string s) =>
        s.Should();            // Compliant

    public object ReturnedByArrowWithConversion(string s) =>
        (object)s.Should();    // Compliant

    public void CalledByArrow(string s) =>
        s.Should();            // Noncompliant

    public void Assigned()
    {
        var s = "Test";
        var assertion = s.Should();  // Compliant
        assertion = s.Should();      // Compliant
    }

    public void PassedAsArgument()
    {
        var s = "Test";
        ValidateString(s.Should());  // Compliant
    }
    private void ValidateString(StringAssertions assertion) { }

    public void UnreducedCall()
    {
        var s = "Test";
        FluentAssertions.AssertionExtensions.Should(s); // Noncompliant
    }

    public void ReturnedInLambda()
    {
        Func<StringAssertions> a = () => "Test".Should();
    }

    public void CustomAssertions()
    {
        var custom = new Custom();
        custom.Should();                    // Noncompliant The custom assertion derives from ReferenceTypeAssertions
        custom.Should().BeCustomAsserted(); // Compliant
    }

    public void CustomStructAssertions()
    {
        var custom = new CustomStruct();
        custom.Should();                    // Compliant Potential FN. CustomStructAssertion does not derive from ReferenceTypeAssertions and is not considered a custom validation.
        custom.Should().BeCustomAsserted(); // Compliant
    }
}

public static class CustomAssertionExtension
{
    public static CustomAssertion Should(this Custom instance)
        => new CustomAssertion(instance);
}

public class CustomAssertion : ReferenceTypeAssertions<Custom, CustomAssertion>
{
    public CustomAssertion(Custom instance)
        : base(instance)
    {
    }

    protected override string Identifier => "custom";

    public void BeCustomAsserted()
    {
        // Not implemented
    }
}

public class Custom { }

public static class CustomStructAssertionExtension
{
    public static CustomStructAssertion Should(this CustomStruct instance)
        => new CustomStructAssertion(instance);
}

public class CustomStructAssertion // Does not derive from ReferenceTypeAssertions
{
    public CustomStructAssertion(CustomStruct instance) { }

    public void BeCustomAsserted()
    {
        // Not implemented
    }
}

public struct CustomStruct { }

