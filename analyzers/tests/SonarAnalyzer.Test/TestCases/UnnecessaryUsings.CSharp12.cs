using Person = (string name, string surname); // FN
using unsafe IntPointerExt = int*; // FN
using IntArray = int[]; // FN
using Point3D = (int, int, int);

namespace ANamespace
{
    using Point2D = (int, int); // FN
    using unsafe IntPointer = int*; // FN: unused
    using StringArray = string[];

    class MyClass
    {
        void AliasType()
        {
            var point = new Point3D(1, 2, 3);
            var stringArray = new StringArray[1];
        }
    }

    namespace BNamespace
    {
        using unsafe IntPointer = int; // Compliant: used

        unsafe class AliasShadowing(IntPointer* y);
    }
}
