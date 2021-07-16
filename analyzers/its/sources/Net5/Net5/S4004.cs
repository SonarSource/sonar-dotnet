using System.Collections;
using System.Collections.Generic;

namespace Net5
{
    public class S4004
    {
        public ArrayList NonGenericList { get; set; }
        public ICollection<string> GenericCollectionInit { get; init; }
    }
}
