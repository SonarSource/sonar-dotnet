using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.Diagnostics
{
    record TestRecord_01 : CollectionBase { } // Noncompliant {{Refactor this collection to implement 'System.Collections.ObjectModel.Collection<T>'.}}
    //     ^^^^^^^^^^^^^

    record TestRecord_02 : IList { }       // Noncompliant
    record TestRecord_03 : IEnumerable { } // Noncompliant
    record TestRecord_04 : ICollection { } // Noncompliant
}
