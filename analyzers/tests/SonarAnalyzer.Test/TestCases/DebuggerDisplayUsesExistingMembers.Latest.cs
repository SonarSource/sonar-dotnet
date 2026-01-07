using System;
using System.Diagnostics;
using Somewhere;

class RawStringLiterals
{
    int SomeProperty => 1;
    int SomeField = 2;

    [DebuggerDisplay("""{SomeProperty}""")] int ExistingMemberTripleQuotes => 1;
    [DebuggerDisplay(""""{SomeField}"""")] int ExistingMemberQuadrupleQuotes => 1;
    [DebuggerDisplay("""
        Some text{SomeField}
        """)] int ExistingMultiLine => 1;
    [DebuggerDisplay($$"""""
        Some text{SomeField}
        """"")] int ExistingMultiLineInterpolated => 1;

    [DebuggerDisplay("""{Nonexistent}""")] int NonexistentTripleQuotes => 1;      // Noncompliant
    //               ^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay(""""{Nonexistent}"""")] int NonexistentQuadrupleQuotes => 1; // Noncompliant
    //               ^^^^^^^^^^^^^^^^^^^^^
    [DebuggerDisplay("""
        Some text{Nonexistent}
        """)] int NonexistentMultiLine1 => 1;                                     // Noncompliant@-2^22#46
    [DebuggerDisplay("""
        Some text{Some
        Property}
        """)] int NonexistentMultiLine2 => 1;                                     // Noncompliant@-3
    [DebuggerDisplay($$"""""
        Some text{Nonexistent}
        """"")] int NonexistentMultiLineInterpolated => 1;                        // Noncompliant@-2
}

public class AccessModifiers
{
    public class BaseClass
    {
        private protected int PrivateProtectedProperty => 1;

        [DebuggerDisplay("{PrivateProtectedProperty}")] // Compliant
        public int SomeProperty => 1;

        [DebuggerDisplay("{Nonexistent}")]              // Noncompliant
        public int OtherProperty => 1;
    }

    public class SubClass : BaseClass
    {
        [DebuggerDisplay("{PrivateProtectedProperty}")] // Compliant
        public int OtherProperty => 1;
    }
}

[DebuggerDisplay("{RecordProperty}")]
public record SomeRecord(int RecordProperty)
{
    [DebuggerDisplay("{RecordProperty}")] public record struct RecordStruct1(int RecordStructProperty);       // Noncompliant
    [DebuggerDisplay("{RecordStructProperty}")] public record struct RecordStruct2(int RecordStructProperty); // Compliant, RecordStructProperty is a property

    [DebuggerDisplay("{RecordProperty}")] public record NestedRecord1(int NestedRecordProperty);       // Noncompliant
    [DebuggerDisplay("{NestedRecordProperty}")] public record NestedRecord2(int NestedRecordProperty); // Compliant, NestedRecordProperty is a property
}

[DebuggerDisplay("{RecordProperty1} bla bla {RecordProperty2}")]
public record struct SomeRecordStruct(int RecordProperty1, string RecordProperty2)
{
    [DebuggerDisplay("{RecordProperty}")]            // Noncompliant
    public class NestedClass1
    {
        [DebuggerDisplay("{NestedClassProperty}")]
        public int NestedClassProperty => 1;
    }

    [DebuggerDisplay("{NestedClassProperty}")]
    public class NestedClass2
    {
        [DebuggerDisplay("{NestedClassProperty}")]
        public int NestedClassProperty => 1;
    }
}

public class ConstantInterpolatedStrings
{
    [DebuggerDisplay($"{{{nameof(SomeProperty)}}}")]
    [DebuggerDisplay($"{{{nameof(NotAProperty)}}}")] // FN: constant interpolated strings not supported
    public int SomeProperty => 1;

    public class NotAProperty { }
}

public interface DefaultInterfaceImplementations
{
    [DebuggerDisplay("{OtherProperty}")]
    [DebuggerDisplay("{OtherPropertyImplemented}")]
    [DebuggerDisplay("{Nonexistent}")]               // Noncompliant
    int WithNonexistentProperty => 1;

    string OtherProperty { get; }
    string OtherPropertyImplemented => "Something";
}

public partial class PartialProperty
{
    public partial string UserName { get; set; }
}

[DebuggerDisplay("{Name}")] // Noncompliant
public partial class PartialProperty
{
    private string _userName;
    public partial string UserName { get => _userName; set { } }
}

[DebuggerDisplay("{UserName}")] // Compliant
public partial class OtherPartialProperty
{
    public partial string UserName { get; set; }
}

public partial class OtherPartialProperty
{
    private string _userName;
    public partial string UserName { get => _userName; set { } }
}

public class EscapeChar
{
    //https://sonarsource.atlassian.net/browse/NET-359
    [DebuggerDisplay("{Non\existent}")] // Noncompliant {{'{Nonxistent}' is not a valid expression. CS1073: Unexpected token ''.}}
    public int SomeProperty => 1;

    [DebuggerDisplay("Test:\e {AnotherProperty}")] // Compliant
    public int AnotherProperty => 1;

    [DebuggerDisplay("Hello\e {Nonexistent}")] // Noncompliant
    public int SomeOtherProperty => 1;

    [DebuggerDisplay("{Nonexistent}")] // Noncompliant
    public int OtherProperty => 1;
}

[DebuggerDisplay("""{Method()}""")]                                                       // Compliant
[DebuggerDisplay("""{Nonexistent()}""")]                                                  // Noncompliant
[DebuggerDisplay("""{Property switch { true => "Yes", false => "No" } }""")]              // Compliant
[DebuggerDisplay("""{Nonexistent switch { true => "Yes", false => "No" } }""")]           // Noncompliant
[DebuggerDisplay("""{Property switch { true => Nonexistent, false => "No" } }""")]        // Noncompliant
[DebuggerDisplay("""{Property switch { true => "Yes", false => Nonexistent } }""")]       // Noncompliant
[DebuggerDisplay("""{Property switch { true => "Yes", false => Nonexistent } ,  nq }""")] // Noncompliant
[DebuggerDisplay("""{Property switch { true => "Yes", false => Method() } } , nq """)]    // Compliant
[DebuggerDisplay("""{Property switch { int i => i } }""")]                                // Compliant
[DebuggerDisplay("""{Property switch { int i => Nonexistent } }""")]                      // FN. The variable designation int i suppresses the processing
[DebuggerDisplay("""{Property is object o && o.ToString() == string.Empty}""")]           // Compliant
public class Expressions
{
    public object Property { get; }
    private string Method() => "";
}

[DebuggerDisplay("{this.ExtensionMethod()}")]                       // Compliant regular extension methods are compatible with DebuggerDisplay NET-2620
[DebuggerDisplay("{this.ExtensionBlockMethod()}")]                  // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{this.ExtensionProperty}")]                       // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{this.BaseExtensionMethod()}")]                   // Compliant regular extension methods are compatible with DebuggerDisplay NET-2620
[DebuggerDisplay("{this.BaseExtensionBlockMethod()}")]              // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{this.BaseExtensionProperty}")]                   // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{this.ExtensionPropertyButNotOnSampleType}")]     // Noncompliant
[DebuggerDisplay("{this.ExtensionMethodButNotOnSampleType()}")]     // Noncompliant
[DebuggerDisplay("{\"baseType\".StringExtensionProp}")]             // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{\"baseType\".StringExtensionMethod()}")]         // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{\"baseType\".StringExtensionProp}")]             // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{StaticClass.StaticExtensionProp}")]              // Compliant extension block members are compatible with DebuggerDisplay
[DebuggerDisplay("{StaticClass.StaticExtensionMethod()}")]          // Compliant extension block members are compatible with DebuggerDisplay
class Sample : Base { }

public class Base { }

static class Extensions
{
    public static string BaseExtensionMethod(this Base b) => "Extension is declared on base class";
    extension(Base b)
    {
        public string BaseExtensionProperty => "Extension Property";
        public string BaseExtensionBlockMethod() => "Extension Method in Extension Block";
    }
    public static string ExtensionMethod(this Sample s) => "Regular Extension Method";
    extension(Sample s)
    {
        public string ExtensionProperty => "Extension Property";
        public string ExtensionBlockMethod() => "Extension Method in Extension Block";
    }

    extension(string s)
    {
        public string StringExtensionProp => "Extension Property";
        public string StringExtensionMethod() => "Extension Method";
    }

    extension(Exception e)
    {
        
        public string ExtensionPropertyButNotOnSampleType => "Extension Property";
        public string ExtensionMethodButNotOnSampleType() => "Extension Method in Extension Block";
    }

    extension(StaticClass)
    {
        public static string StaticExtensionProp => "42";
        public static string StaticExtensionMethod() => "42";
    }
}

static class StaticClass { }

namespace Somewhere
{
    using SomewhereElse;
    [DebuggerDisplay("{this.ExtensionMethod()}")]           // Noncompliant When a static member is outside of the type's namespace DebuggerDisplay will be: "error CS1061: 'BadSample' does not contain a definition for 'ExtensionMethod'"
    [DebuggerDisplay("{this.ExtensionBlockMethod()}")]      // Noncompliant When a static member is outside of the type's namespace DebuggerDisplay will be: "error CS1061: 'BadSample' does not contain a definition for 'ExtensionBlockMethod'"
    [DebuggerDisplay("{this.ExtensionProperty}")]           // Noncompliant When a static member is outside of the type's namespace DebuggerDisplay will be: "error CS1061: 'BadSample' does not contain a definition for 'ExtensionProperty'"
    class BadSample { }

}

namespace SomewhereElse
{
    static class Extensions
    {
        public static string ExtensionMethod(this BadSample s) => "Regular Extension Method";
        extension(BadSample s)
        {
            public string ExtensionProperty => "Extension Property";
            public string ExtensionBlockMethod() => "Extension Method in Extension Block";
        }
    }
}
