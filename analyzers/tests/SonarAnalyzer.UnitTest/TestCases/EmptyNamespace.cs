using System;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    /*1*/
    public class EmptyNamespace
    {

    }
}

namespace Tests.Diagnostics // Noncompliant {{Remove this empty namespace.}}
{
    /*2*/
    using M = Math;
}

namespace Tests.Diagnostics
{
    /*3*/
    using M = Math;
    namespace MyNamespace // Noncompliant
    {
        /*4*/
    }
}

  namespace X { } // Noncompliant
//^^^^^^^^^^^^^^^

namespace Y
{
    namespace Z // Noncompliant
    {

    }
}

