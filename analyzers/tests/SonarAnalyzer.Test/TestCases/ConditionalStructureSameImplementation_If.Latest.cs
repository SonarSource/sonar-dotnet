using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp14
{
    class FieldKeyword
    {
        public int Name
        {
            get
            {
                if(field is 1)
                {                       // Secondary
                    field = field + 1;
                }
                else if(field is 20)
                {                       // Noncompliant
                    field = field + 1;  // Secondary@-1
                }
                else if (field is 50)
                {                       // Noncompliant
                    field = field + 1;
                }
                else if(field is 42)    // Compliant
                {
                    field = 42;
                }
                return field;
            }
            set { }
        }
    }
}
