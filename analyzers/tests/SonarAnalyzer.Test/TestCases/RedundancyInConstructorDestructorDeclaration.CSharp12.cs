// https://github.com/SonarSource/sonar-dotnet/issues/8092
namespace Repro_8092
{
    namespace PrimaryParameterlessConstructor
    {
        class AClassWithBody() { }                                              // Noncompliant {{Remove this redundant primary constructor.}}
//                          ^^
        class AClassWithoutBody();                                              // Noncompliant

        public abstract class BaseClass(string value);
        public abstract class BaseClassNoValue{ };

        public class SomeClass() : BaseClass("SomeValue");                      // Compliant, Foo is calling Base constructor with primary constructor syntax, it's not redundant

        public class DefaultStyle : BaseClass
        {
            public DefaultStyle() : base("SomeValue")                           // Compliant, "default" way of calling Base constructor
            { }
        }

        public class AClass() : BaseClassNoValue;                              // Noncompliant

        public abstract record class BaseRecordClass(string value);

        public record class SomeRecordClass() : BaseRecordClass("SomeValue");  // Compliant, Foo is calling Base constructor with primary constructor syntax, it's not redundant

        public record class DefaultStyleRecordClass : BaseRecordClass
        {
            public DefaultStyleRecordClass() : base("SomeValue")               // Compliant, "default" way of calling Base constructor
            { }
        }

        struct AStructWithBody() { }              // Noncompliant
        struct AStructWithoutBody();              // Noncompliant
        record ARecordWithBody() { }              // Noncompliant
        record ARecordWithoutBody();              // Noncompliant
        record struct ARecordStructWithBody() { } // Noncompliant
        record struct ARecordStructWithoutBody(); // Noncompliant

        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/constructor-errors?f1url=%3FappId%3Droslyn%26k%3Dk(CS8983)#constructors-in-struct-types
        namespace FieldInitializerInStructRequiresConstructor
        {
            struct AStructWithFieldInitializer()              // Compliant
            {
                int aField = 42;
            }

            struct AStructWithFieldDoubleInitializers()       // Compliant
            {
                int aField, bField = 42;
            }

            struct AStructWithPropertyInitializer()           // Compliant
            {
                int AProperty { get; } = 42;
            }

            record struct ARecordStructWithFieldInitializer() // Compliant
            {
                int aField = 42;
            }

            partial struct PartialStructWithPropertyInitializer() // Compliant
            {
            }

            partial struct PartialStructWithPropertyInitializer
            {
                int AProperty { get; } = 42;
            }

            partial record struct PartialRecordStructWithFieldInitializer() // Compliant
            {
            }

            partial record struct PartialRecordStructWithFieldInitializer
            {
                int AField = 42;
            }
        }

        namespace FieldInitializerInClassDontRequireConstructor
        {
            class AClassWithFieldInitializer()    // Noncompliant
            {
                int aField = 42;
            }

            class AClassWithPropertyInitializer() // Noncompliant
            {
                int AProperty { get; } = 42;
            }

            class ARecordWithFieldInitializer()   // Noncompliant
            {
                int aField = 42;
            }
        }
    }
}
