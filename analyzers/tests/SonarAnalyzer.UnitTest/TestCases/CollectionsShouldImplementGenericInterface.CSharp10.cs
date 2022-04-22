using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.Diagnostics
{
    record struct TestRecordStruct_01 : IList { } // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.IList<T>'.}}
    //            ^^^^^^^^^^^^^^^^^^^

    record struct TestRecordStruct_02 : IEnumerable { } // Noncompliant
    record struct TestRecordStruct_03 : ICollection { } // Noncompliant
}
