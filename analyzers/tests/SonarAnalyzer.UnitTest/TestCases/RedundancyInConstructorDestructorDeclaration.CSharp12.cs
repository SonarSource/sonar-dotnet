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
    }
}
