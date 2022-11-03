using System.Collections.Generic;
using System.Numerics;

class IInterface : IAdditionOperators<IInterface, IInterface, IEnumerable<string>>
{
    public static IEnumerable<string> operator +(IInterface left, IInterface right) => null; // FN
}
