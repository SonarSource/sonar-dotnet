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
        Property}")] int WithNonexistentMemberVerbatimMultiLine2 => 1;                           // FN@-1: the new line char make the expression within braces not a valid identifier

    [DebuggerDisplay(ConstantWithInvalidMember)] int WithFormatAsConstant2 => 1;                            // FN: constants are not checked
    [DebuggerDisplay("{Non" + "Existent}")] int WithFormatAsConcatenationOfLiterals => 1;                   // FN: only simple literal supported
    [DebuggerDisplay("{Non"
        + "Existent}")] int WithFormatAsConcatenationOfLiteralsMultiLine => 1;                              // FN: only simple literal supported
    [DebuggerDisplay(ConstantFragment1 + ConstantFragment2)] int WithFormatAsConcatenationOfConstants => 1; // FN: only simple literal supported

    [DebuggerDisplay("{this.NonexistentProperty}")] int PropertyWithExplicitThis => 1;                      // FN: "this." not supported (valid when debugging a C# project)
    [DebuggerDisplay("{Me.NonexistentField}")] int FieldWithExplicitThis => 1;                              // FN: "Me." not supported (valid when debugging a VB.NET project)
    [DebuggerDisplay("{1 + NonexistentProperty}")] int ContainingInvalidMembers => 1;                       // FN: expressions not supported
}

[DebuggerDisplay("{this.ToString()}")]  // Compliant, valid when debugging a C# project
[DebuggerDisplay("{Me.ToString()}")]    // Compliant, valid when debugging a VB.NET project
[DebuggerDisplay("{Nonexistent}")]      // Noncompliant
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

public class InvalidModifier
{
    [DebuggerDisplay("{SomeProperty,asdf}")]
    [DebuggerDisplay("{SomeProperty, asdf}")]
    [DebuggerDisplay("{Nonexistent,asdf}")]  // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
    [DebuggerDisplay("{Nonexistent, asdf}")] // Noncompliant {{'Nonexistent' doesn't exist in this context.}}
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
        [DebuggerDisplayAlias("{Nonexistent}")] int WithAlias => 1; // Noncompliant: attribute name also checked at semantic level
    }
}
