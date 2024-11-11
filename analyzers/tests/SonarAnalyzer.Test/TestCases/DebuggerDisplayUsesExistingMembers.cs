using System;
using System.Diagnostics;

class PropertiesAndFields
{
    const string ConstantWithoutInvalidMembers = "Something";
    const string ConstantWithInvalidMember = "{Nonexistent}";
    const string ConstantFragment1 = "{Non";
    const string ConstantFragment2 = "existent}";

    int SomeProperty => 1;
    int SomeField = 2;
    string[] DeclaredNamespaces = new string[0];

    [DebuggerDisplayAttribute("Hardcoded text")] int WithSuffix => 1;
    [System.Diagnostics.DebuggerDisplay("Hardcoded text")] int WithNamespace => 1;
    [DebuggerDisplay(value: "Hardcoded text")] int WithExplicitParameterName => 1;

    [DebuggerDisplay(null)] int WithEmptyArgList => 1;
    [DebuggerDisplay("")] int WithEmptyFormat => 1;
    [DebuggerDisplay(ConstantWithoutInvalidMembers)] int WithFormatAsConstant1 => 1;
    [DebuggerDisplay(nameof(ConstantWithoutInvalidMembers))] int WithFormatAsNameOf => 1;

    [DebuggerDisplay("{SomeProperty}")] int WithExistingProperty => 1;
    [DebuggerDisplay("{SomeField}")] int WithExistingField => 1;
    [DebuggerDisplay(@"{SomeField}")] int WithExistingFieldVerbatim => 1;
    [DebuggerDisplay(@"Some text
        {SomeField}")] int WithExistingFieldVerbatimMultiLine => 1;

    [DebuggerDisplay("{1 + 1}")] int WithNoMemberReferenced1 => 1;
    [DebuggerDisplay(@"{""1"" + ""1""}")] int WithNoMemberReferenced2 => 1;

    [DebuggerDisplay("{Nonexistent}")] int WithNonexistentMember1 => 1;                          // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^
    [DebuggerDisplay("1 + {Nonexistent}")] int WithNonexistentMember2 => 1;                      // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay("{Nonexistent1} bla bla {Nonexistent2}")] int WithMultipleNonexistent => 1; // Noncompliant {{'Nonexistent1' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay(@"{Nonexistent}")] int WithNonexistentMemberVerbatim => 1;                  // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay(@"Some text
        {Nonexistent}")] int WithNonexistentMemberVerbatimMultiLine1 => 1;                       // Noncompliant@-1^22#34 {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay(@"Some text {Some
        Property}")] int WithNonexistentMemberVerbatimMultiLine2 => 1;                           // Noncompliant@-1
    [DebuggerDisplay(@"Some text {(ParseError1}")] int ParseError1 => 1;                         // Noncompliant {{'{(ParseError1}' is not a valid expression. CS1026: ) expected.}}
    [DebuggerDisplay(@"Some text {int}")] int ParseError2 => 1;                                  // Noncompliant {{'{int}' is not a valid expression. CS1525: Invalid expression term 'int'.}}
    [DebuggerDisplay(@"{ | ParseError3}")] int ParseError3 => 1;                                 // Noncompliant {{'{ | ParseError3}' is not a valid expression. CS1525: Invalid expression term '|'.}}
    [DebuggerDisplay(@"{ ! ParseValid}")] int ParseValid => 1;                                   // Compliant

    [DebuggerDisplay(ConstantWithInvalidMember)] int WithFormatAsConstant2 => 1;                            // Noncompliant
    [DebuggerDisplay("{Non" + "Existent}")] int WithFormatAsConcatenationOfLiterals => 1;                   // Noncompliant
    [DebuggerDisplay("{Non"
        + "Existent}")] int WithFormatAsConcatenationOfLiteralsMultiLine => 1;                              // Noncompliant@-1
    [DebuggerDisplay(ConstantFragment1 + ConstantFragment2)] int WithFormatAsConcatenationOfConstants => 1; // Noncompliant

    [DebuggerDisplay("{this.NonexistentProperty}")] int PropertyWithExplicitThis => 1;                      // Noncompliant
    [DebuggerDisplay("{Me.NonexistentField}")] int FieldWithExplicitThis => 1;                              // Noncompliant {{'Me' doesn't exist in this context.}}
    [DebuggerDisplay("{!NonexistentProperty}")] int UnaryExpression1 => 1;                                  // Noncompliant {{'NonexistentProperty' doesn't exist in this context.}}
    [DebuggerDisplay("{NonexistentProperty!}")] int UnaryExpression2 => 1;                                  // Noncompliant {{'NonexistentProperty' doesn't exist in this context.}}
    [DebuggerDisplay("{1 + NonexistentProperty}")] int BinaryExpression => 1;                               // Noncompliant
    [DebuggerDisplay("{(1 + 1 + (1 + NonexistentProperty))}")] int NestedBinaryExpression => 1;             // Noncompliant
    [DebuggerDisplay("{NonexistentProperty ? 1 : 0}")] int TernaryInCondition => 1;                         // Noncompliant
    [DebuggerDisplay("{true ? NonexistentProperty : 0}")] int TernaryInTrue => 1;                           // Noncompliant
    [DebuggerDisplay("{true ? 1 : NonexistentProperty}")] int TernaryInFalse => 1;                          // Noncompliant
    [DebuggerDisplay("{true ? 1 : (true ? 0 : 1 + NonexistentProperty)}")] int TernaryNested => 1;          // Noncompliant
    [DebuggerDisplay("{Math.Abs(SomeProperty)}")] int StaticMemberAccess => 1;                              // Noncompliant {{'Math' doesn't exist in this context.}} FP
    [DebuggerDisplay("CsdlSemanticsModel({string.Join(\",\", DeclaredNamespaces)})")] int ExpressionWithDoubleQuotes => 1; // Compliant https://sonarsource.atlassian.net/browse/NET-646
}

[DebuggerDisplay("{this.ToString()}")]     // Compliant, valid when debugging a C# project
[DebuggerDisplay("{this.Nonexistent()}")]  // Noncompliant
[DebuggerDisplay("{Me.ToString()}")]       // Noncompliant, valid when debugging a VB.NET project but not in C#
[DebuggerDisplay("{Nonexistent}")]         // Noncompliant
public enum TopLevelEnum { One, Two, Three }

[DebuggerDisplay("{SomeProperty}")]
[DebuggerDisplay("{SomeField}")]
[DebuggerDisplay("{Nonexistent}")]      // Noncompliant
public class NestedTypes
{
    int SomeProperty => 1;
    int SomeField = 1;

    [DebuggerDisplay("{ExistingProperty}")]
    [DebuggerDisplay("{ExistingField}")]
    [DebuggerDisplay("{SomeProperty}")] // Noncompliant
    [DebuggerDisplay("{SomeField}")]    // Noncompliant
    public class NestedClass
    {
        int ExistingProperty => 1;
        int ExistingField => 1;
    }

    [DebuggerDisplay("{ExistingProperty}")]
    [DebuggerDisplay("{ExistingField}")]
    [DebuggerDisplay("{SomeProperty}")] // Noncompliant
    [DebuggerDisplay("{SomeField}")]    // Noncompliant
    public struct NestedStruct
    {
        int ExistingProperty => 1;
        int ExistingField => 1;
    }

    [DebuggerDisplay("{42}")]
    [DebuggerDisplay("{SomeProperty}")] // Noncompliant
    [DebuggerDisplay("{SomeField}")]    // Noncompliant
    public enum NestedEnum { One, Two, Three }
}

public class Delegates
{
    int ExistingProperty => 1;

    [DebuggerDisplay("{ExistingProperty}")] // Noncompliant
    [DebuggerDisplay("{42}")]
    public delegate void Delegate1();
}

public class Indexers
{
    int ExistingProperty => 1;
    int ExistingField => 1;

    [DebuggerDisplay("{ExistingProperty}")]
    [DebuggerDisplay("{ExistingField}")]
    [DebuggerDisplay("{Nonexistent}")] // Noncompliant
    int this[int i] => 1;
}

[DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{Nonexistent}")] // Noncompliant
//                                                                                  ^^^^^^^^^^^^^^^
public class MultipleAttributes
{
    int SomeProperty => 1;
    int SomeField = 1;

    [DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{Nonexistent}")] // Noncompliant
    //                                                                                  ^^^^^^^^^^^^^^^
    int OtherProperty1 => 1;

    [DebuggerDisplay("{Nonexistent1}"), DebuggerDisplay("{Nonexistent2}")]
    //               ^^^^^^^^^^^^^^^^
    //                                                  ^^^^^^^^^^^^^^^^@-1
    int OtherProperty2 => 1;

    [DebuggerDisplay("{Nonexistent1}")][DebuggerDisplay("{Nonexistent2}")]
    //               ^^^^^^^^^^^^^^^^
    //                                                  ^^^^^^^^^^^^^^^^@-1
    int OtherProperty3 => 1;
}

public class CaseSensitivity
{
    int SOMEPROPERTY => 1;
    int SomeProperty => 1;

    [DebuggerDisplay("{SOMEPROPERTY}")]
    [DebuggerDisplay("{SomeProperty}")]
    [DebuggerDisplay("{someProperty}")] // Noncompliant
    [DebuggerDisplay("{someproperty}")] // Noncompliant
    int OtherProperty => 1;
}

public class NonAlphanumericChars
{
    int Aa1_뿓 => 1;

    [DebuggerDisplay("{Aa1_뿓}")]
    [DebuggerDisplay("{Aa1_㤬}")] // Noncompliant {{'Aa1_㤬' doesn't exist in this context.}}
    int SomeProperty1 => 1;
}

public class Whitespaces
{
    [DebuggerDisplay("{ SomeProperty}")]
    [DebuggerDisplay("{SomeProperty }")]
    [DebuggerDisplay("{\tSomeProperty}")]
    [DebuggerDisplay("{\tSomeProperty\t}")]
    [DebuggerDisplay("{ Nonexistent}")]    // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{Nonexistent }")]    // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{\tNonexistent}")]   // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{\tNonexistent\t}")] // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    int SomeProperty => 1;
}

public class NoQuotesModifier
{
    [DebuggerDisplay("{SomeProperty,nq}")]
    [DebuggerDisplay("{SomeProperty ,nq}")]
    [DebuggerDisplay("{SomeProperty, nq}")]
    [DebuggerDisplay("{SomeProperty,nq }")]
    [DebuggerDisplay("{Nonexistent,nq}")]  // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{Nonexistent ,nq}")] // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{Nonexistent, nq}")] // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{Nonexistent,nq }")] // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    int SomeProperty => 1;
}

public class OtherModifier
{
    [DebuggerDisplay("{SomeProperty,d}")]     // Compliant https://sonarsource.atlassian.net/browse/NET-646
    [DebuggerDisplay("{SomeProperty,raw}")]   // Compliant https://sonarsource.atlassian.net/browse/NET-646
    [DebuggerDisplay("{SomeProperty,asdf}")]  // Compliant, the qualifiers are not specified in the documentation. We assume any word is valid here
    [DebuggerDisplay("{SomeProperty, asdf}")] // Compliant
    [DebuggerDisplay("{Nonexistent,asdf}")]   // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{Nonexistent, asdf}")]  // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{Nonexistent, a12f}")]  // Noncompliant {{'{Nonexistent, a12f}' is not a valid expression. CS1073: Unexpected token ','.}}
    int SomeProperty => 1;
}

public class OptionalAttributeParameter
{
    [DebuggerDisplay("{SomeProperty}", Name = "Any name")]
    [DebuggerDisplay("{Nonexistent}", Name = "Any name")]                                              // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^
    [DebuggerDisplay("{Nonexistent}", Name = "Any name", Type = nameof(OptionalAttributeParameter))]   // Noncompliant
    //               ^^^^^^^^^^^^^^^
    [DebuggerDisplay("{Nonexistent}", Name = "Any name", Target = typeof(OptionalAttributeParameter))] // Noncompliant
    //               ^^^^^^^^^^^^^^^
    int SomeProperty => 1;
}

public class Inheritance
{
    public class BaseClass
    {
        int SomeProperty => 1;
    }

    public class SubClass : BaseClass
    {
        [DebuggerDisplay("{SomeProperty}")] // Compliant, defined in base class
        int OtherProperty => 1;
    }
}

public class AccessModifiers
{
    public class BaseClass
    {
        public int PublicProperty => 1;
        internal int InternalProperty => 1;
        protected int ProtectedProperty => 1;
        private int PrivateProperty => 1;

        [DebuggerDisplay("{PublicProperty}")]
        [DebuggerDisplay("{InternalProperty}")]
        [DebuggerDisplay("{ProtectedProperty}")]
        [DebuggerDisplay("{PrivateProperty}")]
        int SomeProperty => 1;
    }

    public class SubClass : BaseClass
    {
        [DebuggerDisplay("{PublicProperty}")]
        [DebuggerDisplay("{InternalProperty}")]
        [DebuggerDisplay("{ProtectedProperty}")]
        [DebuggerDisplay("{PrivateProperty}")]
        int OtherProperty => 1;
    }
}

public class AttributeTargets
{
    [global:DebuggerDisplay("{Nonexistent}")]    // Noncompliant, attribute at global level, referencing a non-existent member
    [assembly: DebuggerDisplay("{Nonexistent}")] // Noncompliant, attribute at assembly level, referencing a non-existent member
    [field: DebuggerDisplay("{Nonexistent}")]    // Noncompliant, attribute ignored, still referencing a non-existent member
    [property: DebuggerDisplay("{Nonexistent}")] // Noncompliant, attribute taken into account
    int SomeProperty => 1;
}

public class InvalidAttributes
{
    [DebuggerDisplay] int NoArgs => 1;                                         // Error [CS7036]
    [DebuggerDisplay(Type = nameof(InvalidAttributes))] int MissingValue => 1; // Error [CS7036]
}

namespace WithTypeAlias
{
    using DebuggerDisplayAlias = System.Diagnostics.DebuggerDisplayAttribute;

    public class Test
    {
        [DebuggerDisplayAlias("{Nonexistent}")] int WithAlias => 1; // FN: Aliases are not supported for performance reasons
    }
}

// https://sonarsource.atlassian.net/browse/NET-575
public class Repo_Net575
{
    [DebuggerDisplay("{Property}",
        Name = "{Nonexistent}",  // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
        Type = "{Nonexistent}")] // Noncompliant
    public int Property { get; }

    [DebuggerDisplay("{Property1}",
        Type = "{Type}")] // Noncompliant TP https://sonarsource.atlassian.net/browse/NET-646 (Checked in VS: "Type" can not be evaluated by the compiler)
    public int Property1 { get; }
}

// Noncompliant@+1 {{'Error' doesn't exist in this context.}}
[DebuggerDisplay(@"
    Some text
    {
        Property == 0
            ? Error
            : ""Okay""
    }
")]
public class Multiline
{
    public int Property { get; }
}

[DebuggerDisplay("{this.Recurse}")]
[DebuggerDisplay("{Recurse}")]
[DebuggerDisplay("{Recurse.Recurse}")]
[DebuggerDisplay("{Recurse.Recurse.Recurse}")]
[DebuggerDisplay("{Recurse.Recurse.Method()}")]
[DebuggerDisplay("{Recurse.Method().Recurse}")]
[DebuggerDisplay("{Method().Recurse.Recurse}")]
[DebuggerDisplay("{Nonexistent.Recurse.Recurse}")]      // Noncompliant
[DebuggerDisplay("{this.Nonexistent.Recurse.Recurse}")] // Noncompliant
[DebuggerDisplay("{Recurse.Nonexistent.Recurse}")]      // Compliant: Only the most left hand side of a member access is detected
[DebuggerDisplay("{Recurse.Recurse.Nonexistent}")]      // Compliant
[DebuggerDisplay("{Nonexistent?.Recurse}")]             // Noncompliant
[DebuggerDisplay("{Recurse ?? Recurse}")]               // Compliant
[DebuggerDisplay("{Nonexistent ?? Recurse}")]           // Noncompliant
[DebuggerDisplay("{Recurse ?? Nonexistent}")]           // Noncompliant
public class Recursion
{
    public Recursion Recurse { get; }
    public void Method() { }
}
