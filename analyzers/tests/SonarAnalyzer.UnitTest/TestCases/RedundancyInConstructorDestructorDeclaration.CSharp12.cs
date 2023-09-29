// https://github.com/SonarSource/sonar-dotnet/issues/8092
namespace Repro_8092
{
    namespace PrimaryParameterlessConstructor
    {
        class AClassWithBody() { }                // FN
        class AClassWithoutBody();                // FN
        struct AStructWithBody() { }              // FN
        struct AStructWithoutBody();              // FN
        record ARecordWithBody() { }              // FN
        record ARecordWithoutBody();              // FN
        record struct ARecordStructWithBody() { } // FN
        record struct ARecordStructWithoutBody(); // FN

        namespace FieldInitializerInStructRequiresConstructor
        {
            struct AStructWithFieldInitializer()              // Compliant
            {
                public int aField = 42;
            }

            struct AStructWithPropertyInitializer()           // Compliant
            {
                public int AProperty { get; } = 42;
            }

            record struct ARecordStructWithFieldInitializer() // Compliant
            {
                public int aField = 42;
            }
        }

        namespace FieldInitializerInClassDontRequireConstructor
        {
            class AClassWithFieldInitializer()       // FN
            {
                public int aField = 42;
            }

            class AClassWithPropertyInitializer()    // FN
            {
                public int AProperty { get; } = 42;
            }

            class ARecordWithFieldInitializer()      // FN
            {
                public int aField = 42;
            }
        }
    }
}
