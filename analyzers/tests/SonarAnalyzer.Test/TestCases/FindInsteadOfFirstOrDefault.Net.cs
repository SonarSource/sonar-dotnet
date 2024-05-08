using System.Collections.Generic;
using System.Linq;
using System;

public class FindInsteadOfFirstOrDefault
{
    public void List(List<int> data)
    {
        data.FirstOrDefault(x => true);                 // Noncompliant
        data.FirstOrDefault(default(int));              // Compliant
        data.FirstOrDefault(x => false, default(int));  // Compliant
    }

    public void Array(int[] data)
    {
        data.FirstOrDefault(x => true);                 // Noncompliant
        data.FirstOrDefault(default(int));              // Compliant
        data.FirstOrDefault(x => false, default(int));  // Compliant
    }
}
