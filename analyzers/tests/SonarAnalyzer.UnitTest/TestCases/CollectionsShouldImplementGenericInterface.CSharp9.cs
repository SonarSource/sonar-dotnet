using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.Diagnostics
{
    record TestRecord_01 : CollectionBase { } // FN

    record TestRecord_02 : IList { } // FN

    record TestRecord_03 : IEnumerable { } // FN

    record TestRecord_04 : ICollection { } // FN
}
