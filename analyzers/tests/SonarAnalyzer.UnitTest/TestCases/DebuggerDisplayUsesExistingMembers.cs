using System;
using System.Diagnostics;

class TestOnPropertiesAndFields
{
    const string ConstantWithoutInvalidMembers = "1";
    const string ConstantWithInvalidMember = "{NonExisting}";
    const string ConstantFragment1 = "{Non";
    const string ConstantFragment2 = "Existing}";

    int SomeProperty => 1;
    int SomeField = 2;

    [DebuggerDisplayAttribute("1")] int WithSuffix => 1;
    [System.Diagnostics.DebuggerDisplay("1")] int WithNamespace => 1;
    [DebuggerDisplay(value: "1")] int WithExplicitParameterName => 1;

    [DebuggerDisplay(null)] int WithEmptyArgList => 1;
    [DebuggerDisplay("")] int WithEmptyFormat => 1;
    [DebuggerDisplay(ConstantWithoutInvalidMembers)] int WithFormatAsConstant1 => 1;
    [DebuggerDisplay(nameof(ConstantWithoutInvalidMembers))] int WithFormatAsNameOf => 1;

    [DebuggerDisplay("{SomeProperty}")] int WithExistingProperty => 1;
    [DebuggerDisplay("{SomeField}")] int WithExistingField => 1;
    [DebuggerDisplay(@"{SomeField}")] int WithExistingFieldVerbatim => 1;
    [DebuggerDisplay("{1 + 1}")] int WithNoMemberReferenced1 => 1;
    [DebuggerDisplay(@"{""1"" + ""1""}")] int WithNoMemberReferenced2 => 1;

    [DebuggerDisplay("{NonExisting}")] int WithNonExistingMember1 => 1;                          // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^
    [DebuggerDisplay("1 + {NonExisting}")] int WithNonExistingMember2 => 1;                      // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay("{NonExisting1} bla bla {NonExisting2}")] int WithMultipleNonExisting => 1; // Noncompliant {{'NonExisting1' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay(@"{NonExisting}")] int WithNonExistingMemberVerbatim => 1;                  // Noncompliant {{'NonExisting' doesn't exist in this context.}}

    [DebuggerDisplay(ConstantWithInvalidMember)] int WithFormatAsConstant2 => 1;                            // Compliant, FN: constants are not checked
    [DebuggerDisplay("{Non" + "Existing}")] int WithFormatAsConcatenationOfLiterals => 1;                   // Compliant, FN: only simple literal supported
    [DebuggerDisplay(ConstantFragment1 + ConstantFragment2)] int WithFormatAsConcatenationOfConstants => 1; // Compliant, FN: only simple literal supported

    [DebuggerDisplay("{this.NonExistingProperty}")] int PropertyWithExplicitThis => 1;                      // Compliant, FN: "this." not supported
    [DebuggerDisplay("{this.NonExistingField}")] int FieldWithExplicitThis => 1;                            // Compliant, FN: "this." not supported
    [DebuggerDisplay("{1 + NonExistingProperty}")] int ContainingInvalidMembers => 1;                       // Compliant, FN: expressions not supported
}

[DebuggerDisplay("{this.ToString()}")]  // Compliant
[DebuggerDisplay("{NonExisting}")]      // Noncompliant {{'NonExisting' doesn't exist in this context.}}
public enum TopLevelEnum { One, Two, Three }

[DebuggerDisplay("{SomeProperty}")]
[DebuggerDisplay("{SomeField}")]
[DebuggerDisplay("{NonExisting}")]      // Noncompliant {{'NonExisting' doesn't exist in this context.}}
public class TestOnNestedTypes
{
    int SomeProperty => 1;
    int SomeField = 1;

    [DebuggerDisplay("{ExistingProperty}")]
    [DebuggerDisplay("{ExistingField}")]
    [DebuggerDisplay("{SomeProperty}")] // Noncompliant {{'SomeProperty' doesn't exist in this context.}}
    [DebuggerDisplay("{SomeField}")]    // Noncompliant {{'SomeField' doesn't exist in this context.}}
    public class NestedClass
    {
        int ExistingProperty => 1;
        int ExistingField => 1;
    }

    [DebuggerDisplay("{ExistingProperty}")]
    [DebuggerDisplay("{ExistingField}")]
    [DebuggerDisplay("{SomeProperty}")] // Noncompliant {{'SomeProperty' doesn't exist in this context.}}
    [DebuggerDisplay("{SomeField}")]    // Noncompliant {{'SomeField' doesn't exist in this context.}}
    public struct NestedStruct
    {
        int ExistingProperty => 1;
        int ExistingField => 1;
    }

    public enum NestedEnum { One, Two, Three }
}

public class TestOnDelegates
{
    int ExistingProperty => 1;

    [DebuggerDisplay("{ExistingProperty}")] // Noncompliant
    [DebuggerDisplay("{1}")]                // Compliant
    public delegate void Delegate1();
}

public class TestOnIndexers
{
    int ExistingProperty => 1;
    int ExistingField => 1;

    [DebuggerDisplay("{ExistingProperty}")]
    [DebuggerDisplay("{ExistingField}")]
    [DebuggerDisplay("{NonExisting}")] // Noncompliant
    int this[int i] => 1;
}

[DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{NonExisting}")] // Noncompliant
//                                                                                  ^^^^^^^^^^^^^^^
public class TestMultipleAttributes
{
    int SomeProperty => 1;
    int SomeField = 1;

    [DebuggerDisplay("{SomeProperty}"), DebuggerDisplay("{SomeField}"), DebuggerDisplay("{NonExisting}")] // Noncompliant
    //                                                                                  ^^^^^^^^^^^^^^^
    int OtherProperty1 => 1;

    [DebuggerDisplay("{NonExisting1}"), DebuggerDisplay("{NonExisting2}")]
    //               ^^^^^^^^^^^^^^^^
    //                                                  ^^^^^^^^^^^^^^^^@-1
    int OtherProperty2 => 1;

    [DebuggerDisplay("{NonExisting1}")][DebuggerDisplay("{NonExisting2}")]
    //               ^^^^^^^^^^^^^^^^
    //                                                  ^^^^^^^^^^^^^^^^@-1
    int OtherProperty3 => 1;
}

public class SupportCaseSensitivity
{
    int SOMEPROPERTY => 1;
    int SomeProperty => 1;

    [DebuggerDisplay("{SOMEPROPERTY}")]  // Compliant
    [DebuggerDisplay("{SomeProperty}")]  // Compliant
    [DebuggerDisplay("{someProperty}")]  // Noncompliant {{'someProperty' doesn't exist in this context.}}
    [DebuggerDisplay("{someproperty}")]  // Noncompliant {{'someproperty' doesn't exist in this context.}}
    int OtherProperty => 1;
}

public class SupportNonAlphanumericChars
{
    int Aa1_뿓 => 1;

    [DebuggerDisplay("{Aa1_뿓}")]  // Compliant
    [DebuggerDisplay("{Aa1_㤬}")]  // Noncompliant {{'Aa1_㤬' doesn't exist in this context.}}
    int SomeProperty1 => 1;
}

public class SupportWhitespaces
{
    [DebuggerDisplay("{ SomeProperty}")]
    [DebuggerDisplay("{SomeProperty }")]
    [DebuggerDisplay("{\tSomeProperty}")]
    [DebuggerDisplay("{\tSomeProperty\t}")]
    [DebuggerDisplay("{ NonExisting}")]    // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    [DebuggerDisplay("{NonExisting }")]    // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    [DebuggerDisplay("{\tNonExisting}")]   // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    [DebuggerDisplay("{\tNonExisting\t}")] // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    int SomeProperty => 1;
}

public class SupportNq
{
    [DebuggerDisplay("{SomeProperty,nq}")]
    [DebuggerDisplay("{SomeProperty ,nq}")]
    [DebuggerDisplay("{SomeProperty, nq}")]
    [DebuggerDisplay("{SomeProperty,nq }")]
    [DebuggerDisplay("{NonExisting,nq}")]  // Noncompliant
    [DebuggerDisplay("{NonExisting ,nq}")] // Noncompliant
    [DebuggerDisplay("{NonExisting, nq}")] // Noncompliant
    [DebuggerDisplay("{NonExisting,nq }")] // Noncompliant
    int SomeProperty => 1;
}

public class SupportOptionalAttributeParameter
{
    [DebuggerDisplay("{SomeProperty}", Name = "Any name")]                                                    // Compliant
    [DebuggerDisplay("{NonExisting}", Name = "Any name")]                                                     // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^
    [DebuggerDisplay("{NonExisting}", Name = "Any name", Type = nameof(SupportOptionalAttributeParameter))]   // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^
    [DebuggerDisplay("{NonExisting}", Name = "Any name", Target = typeof(SupportOptionalAttributeParameter))] // Noncompliant {{'NonExisting' doesn't exist in this context.}}
    //               ^^^^^^^^^^^^^^^
    int SomeProperty => 1;
}

public class SupportInheritance
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

public class SupportAccessModifiers
{
    public class BaseClass
    {
        public int PublicProperty => 1;
        internal int InternalProperty => 1;
        protected int ProtectedProperty => 1;
        private int PrivateProperty => 1;

        [DebuggerDisplay("{PublicProperty}")]           // Compliant
        [DebuggerDisplay("{InternalProperty}")]         // Compliant
        [DebuggerDisplay("{ProtectedProperty}")]        // Compliant
        [DebuggerDisplay("{PrivateProperty}")]          // Compliant
        int SomeProperty => 1;
    }

    public class SubClass : BaseClass
    {
        [DebuggerDisplay("{PublicProperty}")]           // Compliant
        [DebuggerDisplay("{InternalProperty}")]         // Compliant
        [DebuggerDisplay("{ProtectedProperty}")]        // Compliant
        [DebuggerDisplay("{PrivateProperty}")]          // Compliant
        int OtherProperty => 1;
    }
}

public class SupportAttributeTargets
{
    [assembly: DebuggerDisplay("{NonExisting}")] // Noncompliant, attribute at assembly level, referencing a non-existing member
    [field: DebuggerDisplay("{NonExisting}")]    // Noncompliant, attribute ignored, still referencing a non-existing member
    [property: DebuggerDisplay("{NonExisting}")] // Noncompliant, attribute taken into account
    int SomeProperty => 1;
}

namespace WithTypeAlias
{
    using DebuggerDisplayAlias = System.Diagnostics.DebuggerDisplayAttribute;

    public class Test
    {
        [DebuggerDisplayAlias("{NonExisting}")] int WithAlias => 1; // Compliant, FN: attribute name checked at syntax level
    }
}
