using System;

namespace CSharp14
{
    public partial class Person
    {
        public partial Person(DateTime birthday)
        {
            expectedFingers = 5;    // Noncompliant
        }
    }
}
