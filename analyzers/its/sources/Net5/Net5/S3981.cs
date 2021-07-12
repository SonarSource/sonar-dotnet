using System.Collections.Generic;
using System.Linq;

namespace Net5
{
    public class S3981
    {
        public void Foo()
        {
            const int localConst_Zero = 0;
            var someEnumerable = new List<string>();
            if (someEnumerable.Count() is  >= localConst_Zero)
            {
            }
        }
    }
}
