/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Test.Rules;

public partial class UnusedPrivateMemberTest
{
    [TestMethod]
    public void UnusedPrivateMember_Constructor_Accessibility() =>
        builder.AddSnippet(@"
public class PrivateConstructors
{
    private PrivateConstructors(int i) { var x = 5; } // Noncompliant {{Remove the unused private constructor 'PrivateConstructors'.}}
//          ^^^^^^^^^^^^^^^^^^^
    static PrivateConstructors() { var x = 5; }

    private class InnerPrivateClass // Noncompliant
    {
        internal InnerPrivateClass(int i) { var x = 5; } // Noncompliant
        protected InnerPrivateClass(string s) { var x = 5; } // Noncompliant
        protected internal InnerPrivateClass(double d) { var x = 5; } // Noncompliant
        public InnerPrivateClass(char c) { var x = 5; } // Noncompliant
    }

    private class OtherPrivateClass // Noncompliant
    {
        private OtherPrivateClass() { var x = 5; } // Noncompliant
    }
}

public class NonPrivateMembers
{
    internal NonPrivateMembers(int i) { var x = 5; }
    protected NonPrivateMembers(string s) { var x = 5; }
    protected internal NonPrivateMembers(double d) { var x = 5; }
    public NonPrivateMembers(char c) { var x = 5; }

    public class InnerPublicClass
    {
        internal InnerPublicClass(int i) { var x = 5; }
        protected InnerPublicClass(string s) { var x = 5; }
        protected internal InnerPublicClass(double d) { var x = 5; }
        public InnerPublicClass(char c) { var x = 5; }
    }
}
").Verify();

    [TestMethod]
    public void UnusedPrivateMember_Constructor_DirectReferences() =>
        builder.AddSnippet("""
            public abstract class PrivateConstructors
            {
                public class Constructor1
                {
                    public static readonly Constructor1 Instance = new Constructor1();
                    private Constructor1() { var x = 5; }
                }

                public class Constructor2
                {
                    public Constructor2(int a) { }
                    private Constructor2() { var x = 5; } // Compliant - FN
                }

                public class Constructor3
                {
                    public Constructor3(int a) : this() { }
                    private Constructor3() { var x = 5; }
                }

                public class Constructor4
                {
                    static Constructor4() { var x = 5; }
                }
            }

            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Constructor_Inheritance() =>
        builder.AddSnippet(@"
public class Inheritance
{
    private abstract class BaseClass1
    {
        protected BaseClass1() { var x = 5; }
    }

    private class DerivedClass1 : BaseClass1 // Noncompliant {{Remove the unused private class 'DerivedClass1'.}}
    {
        public DerivedClass1() : base() { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/1398
    private abstract class BaseClass2
    {
        protected BaseClass2() { var x = 5; }
    }

    private class DerivedClass2 : BaseClass2 // Noncompliant {{Remove the unused private class 'DerivedClass2'.}}
    {
        public DerivedClass2() { }
    }
}
").Verify();

    [TestMethod]
    public void UnusedPrivateMember_Empty_Constructors() =>
        builder.AddSnippet("""
            public class PrivateConstructors
            {
                private PrivateConstructors(int i) { } // Compliant, empty ctors are reported from another rule
            }

            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Illegal_Interface_Constructor() =>
        // While typing code in IDE, we can end up in a state where an interface has a constructor defined.
        // Even though this results in a compiler error (CS0526), IDE will still trigger rules on the interface.
        builder.AddSnippet(@"
public interface IInterface
{
    // UnusedPrivateMember rule does not trigger AD0001 error from NullReferenceException
    IInterface() {} // Error [CS0526]
}
").Verify();

    [TestMethod]
    [DataRow("private", "Remove the unused private constructor 'Foo'.")]
    [DataRow("protected", "Remove unused constructor of private type 'Foo'.")]
    [DataRow("internal", "Remove unused constructor of private type 'Foo'.")]
    [DataRow("public", "Remove unused constructor of private type 'Foo'.")]
    public void UnusedPrivateMember_NonPrivateConstructorInPrivateClass(string accessModifier, string expectedMessage) =>
        builder.AddSnippet($$$"""
public class Some
{
    private class Foo // Noncompliant
    {
        {{{accessModifier}}} Foo() // Noncompliant {{{{{expectedMessage}}}}}
        {
            var a = 1;
        }
    }
}
""").Verify();

    [TestMethod]
    public void UnusedPrivateMember_RecordPositionalConstructor() =>
        builder.AddSnippet("""
            // https://github.com/SonarSource/sonar-dotnet/issues/5381
            public abstract record Foo
            {
                Foo(string value)
                {
                    Value = value;
                }

                public string Value { get; }

                public sealed record Bar(string Value) : Foo(Value);
            }
            """).WithOptions(LanguageOptions.FromCSharp9).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_NonExistentRecordPositionalConstructor() =>
    builder.AddSnippet(@"
public abstract record Foo
{
    public sealed record Bar(string Value) : RandomRecord(Value); // Error [CS0246, CS1729] no suitable method found to override
}").WithOptions(LanguageOptions.FromCSharp10).Verify();

    // Tests private nested sealed class with [Export] attribute - a valid MEF pattern used in VS extensions
    // Production example: https://github.com/NoahRic/EditorItemTemplates/blob/master/ClassifierTemplate.cs
    [TestMethod]
    public void UnusedPrivateMember_Constructor_MefExportOnType() =>
        builder.AddSnippet("""
            using System.ComponentModel.Composition;

            public interface IFormatter
            {
                string Format(string input);
            }

            public class FormatterContainer
            {
                [Export(typeof(IFormatter))]
                private sealed class MefExportedFormatter : IFormatter // Compliant - MEF exported type
                {
                    private readonly string _prefix;

                    MefExportedFormatter() => _prefix = "Formatted: "; // Compliant - MEF instantiates via reflection

                    public string Format(string input) => _prefix + input;
                }
            }
            """)
            .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
            .VerifyNoIssues();

    // Tests InheritedExport on interface - implementing classes automatically inherit the export
    // Per MS docs: "an interface can be decorated with an InheritedExport attribute at the interface level,
    // and that export along with any associated metadata will be inherited by any implementing classes"
    // Source: https://learn.microsoft.com/en-us/dotnet/framework/mef/attributed-programming-model-overview-mef
    [TestMethod]
    public void UnusedPrivateMember_Constructor_MefInheritedExportOnInterface() =>
        builder.AddSnippet("""
            using System.ComponentModel.Composition;

            [InheritedExport(typeof(IClassifier))]
            public interface IClassifier
            {
                string Classify(string input);
            }

            public class ClassifierContainer
            {
                private sealed class SimpleClassifier : IClassifier // Compliant - implements InheritedExport interface
                {
                    private readonly string _prefix;

                    SimpleClassifier() => _prefix = "Classified: "; // Compliant - MEF instantiates via reflection

                    public string Classify(string input) => _prefix + input;
                }
            }
            """)
            .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
            .VerifyNoIssues();

    // Tests InheritedExport on base class - subclasses automatically inherit and provide the same export
    // Per MS docs: "a part can export itself by using the InheritedExport attribute.
    // Subclasses of the part will inherit and provide the same export, including contract name and contract type"
    // Source: https://learn.microsoft.com/en-us/dotnet/framework/mef/attributed-programming-model-overview-mef
    [TestMethod]
    public void UnusedPrivateMember_Constructor_MefInheritedExportOnBaseClass() =>
        builder.AddSnippet("""
            using System.ComponentModel.Composition;

            [InheritedExport(typeof(HandlerBase))]
            public abstract class HandlerBase
            {
                public abstract string Handle(string input);
            }

            public class HandlerContainer
            {
                private sealed class SimpleHandler : HandlerBase // Compliant - derives from InheritedExport base
                {
                    private readonly string _tag;

                    SimpleHandler() => _tag = "[handled] "; // Compliant - MEF instantiates via reflection

                    public override string Handle(string input) => _tag + input;
                }
            }
            """)
            .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
            .VerifyNoIssues();

    // Tests MEF2 (System.Composition) Export attribute - same pattern as MEF1 but from the newer lightweight composition API
    [TestMethod]
    public void UnusedPrivateMember_Constructor_Mef2ExportOnType() =>
        builder.AddSnippet("""
            using System.Composition;

            public interface IProcessor
            {
                string Process(string input);
            }

            public class ProcessorContainer
            {
                [Export(typeof(IProcessor))]
                private sealed class SimpleProcessor : IProcessor // Compliant - MEF2 exported type
                {
                    private readonly string _suffix;

                    SimpleProcessor() => _suffix = " [processed]"; // Compliant - MEF2 instantiates via reflection

                    public string Process(string input) => input + _suffix;
                }
            }
            """)
            .AddReferences(MetadataReferenceFacade.SystemCompositionAttributedModel)
            .VerifyNoIssues();

    // Tests custom attribute derived from ExportAttribute - MEF recognizes derived export attributes
    // Per MS docs: custom attributes inheriting from ExportAttribute can include metadata as properties
    // Source: https://learn.microsoft.com/en-us/dotnet/framework/mef/attributed-programming-model-overview-mef
    [TestMethod]
    public void UnusedPrivateMember_Constructor_MefCustomExportAttribute() =>
        builder.AddSnippet("""
            using System.ComponentModel.Composition;

            public class MyCustomExportAttribute : ExportAttribute { }

            public class PluginContainer
            {
                [MyCustomExport]
                private sealed class CustomPlugin // Compliant - custom Export-derived attribute
                {
                    CustomPlugin() { var x = 1; } // Compliant - MEF instantiates via reflection
                }
            }
            """)
            .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
            .VerifyNoIssues();

    // Tests InheritedExport on generic interface - implementing classes inherit the export
    // MEF supports generic types: https://www.codeproject.com/Articles/323919/MEF-Generics
    [TestMethod]
    public void UnusedPrivateMember_Constructor_MefInheritedExportOnGenericInterface() =>
        builder.AddSnippet("""
            using System.ComponentModel.Composition;

            [InheritedExport(typeof(IHandler<>))]
            public interface IHandler<T>
            {
                void Handle(T item);
            }

            public class Container
            {
                private sealed class StringHandler : IHandler<string> // Compliant - implements InheritedExport generic interface
                {
                    StringHandler() { var x = 1; } // Compliant - MEF instantiates via reflection
                    public void Handle(string item) { }
                }
            }
            """)
            .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
            .VerifyNoIssues();

    // Tests InheritedExport on generic base class - derived classes inherit the export
    [TestMethod]
    public void UnusedPrivateMember_Constructor_MefInheritedExportOnGenericBaseClass() =>
        builder.AddSnippet("""
            using System.ComponentModel.Composition;

            [InheritedExport(typeof(ProcessorBase<>))]
            public abstract class ProcessorBase<T>
            {
                public abstract void Process(T item);
            }

            public class Container
            {
                private sealed class IntProcessor : ProcessorBase<int> // Compliant - derives from InheritedExport generic base
                {
                    IntProcessor() { var x = 1; } // Compliant - MEF instantiates via reflection
                    public override void Process(int item) { }
                }
            }
            """)
            .AddReferences(MetadataReferenceFacade.SystemComponentModelComposition)
            .VerifyNoIssues();
}
