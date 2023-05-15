using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;


public class FindInsteadOfFirstOrDefault
{
    public void ArrayType()
    {
        var data = new int[1];

        _ = data.FirstOrDefault(x => true); // Noncompliant
        //       ^^^^^^^^^^^^^^
        _ = Array.Find(data, x => true); // Compliant

        _ = data.FirstOrDefault(); // Compliant

        data.Select(x => x * 2).ToArray().FirstOrDefault(x => true); // Noncompliant
        //                                ^^^^^^^^^^^^^^
        Array.Find(data.Select(x => x * 2).ToArray(), x => true); // Compliant

        int[] DoWork() => null;

        _ = DoWork().FirstOrDefault(x => true); // Noncompliant
        //           ^^^^^^^^^^^^^^
        _ = Array.Find(DoWork(), x => true); // Compliant

        _ = new Func<int[], int>(list => list.FirstOrDefault(x => true)); // Noncompliant
        //                                    ^^^^^^^^^^^^^^

        var lambda = new Func<int[]>(() => new int[1]);

        _ = lambda().FirstOrDefault(x => true); // Noncompliant
        //           ^^^^^^^^^^^^^^
        _ = Array.Find(lambda(), x => true); // Compliant
    }
}
