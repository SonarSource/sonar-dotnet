using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.Diagnostics
{
    record struct TestRecordStruct_01 : IList { } // FN

    record struct TestRecordStruct_02 : IEnumerable { } // FN

    record struct TestRecordStruct_03 : ICollection { } // FN
}
