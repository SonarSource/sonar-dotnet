// https://github.com/SonarSource/sonar-dotnet/issues/8092
namespace Repro_8092
{
    namespace PrimaryParameterlessConstructor
    {
        class AClassWithBody() { }                // Noncompliant {{Remove this redundant primary constructor.}}
//                          ^^
        class AClassWithoutBody();                // Noncompliant
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
