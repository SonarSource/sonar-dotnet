using System.Collections.Generic;
using System.Linq;
using System;

public class FindInsteadOfFirstOrDefault
{
    public void List(List<int> data)
    {
        _ = data.FirstOrDefault(default(int)); // Compliant
        _ = data.FirstOrDefault(x => false, default(int)); // Compliant
    }

    public void Array(int[] data)
    {
        _ = data.FirstOrDefault(default(int)); // Compliant
        _ = data.FirstOrDefault(x => false, default(int)); // Compliant
    }
}
