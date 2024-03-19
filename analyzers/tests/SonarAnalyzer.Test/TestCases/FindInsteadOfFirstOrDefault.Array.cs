using System.Linq;
using System;

public class FindInsteadOfFirstOrDefault
{
    public void ArrayType()
    {
        var data = new int[1];

        data.FirstOrDefault(x => true); // Noncompliant {{"Array.Find" static method should be used instead of the "FirstOrDefault" extension method.}}
        //   ^^^^^^^^^^^^^^
        Array.Find(data, x => true); // Compliant

        data.FirstOrDefault(); // Compliant

        data.Select(x => x * 2).ToArray().FirstOrDefault(x => true); // Noncompliant
        //                                ^^^^^^^^^^^^^^
        Array.Find(data.Select(x => x * 2).ToArray(), x => true); // Compliant

        int[] DoWork() => null;

        DoWork().FirstOrDefault(x => true); // Noncompliant
        //       ^^^^^^^^^^^^^^
        Array.Find(DoWork(), x => true); // Compliant

        _ = new Func<int[], int>(list => list.FirstOrDefault(x => true)); // Noncompliant
        //                                    ^^^^^^^^^^^^^^

        var lambda = new Func<int[]>(() => new int[1]);

        lambda().FirstOrDefault(x => true); // Noncompliant
        //       ^^^^^^^^^^^^^^
        Array.Find(lambda(), x => true); // Compliant
    }

    public static void SpecialPattern(int[] data)
    {
        (true ? data : data).FirstOrDefault(x => true); // Noncompliant
        //                   ^^^^^^^^^^^^^^
        (data ?? data).FirstOrDefault(x => true); // Noncompliant
        //             ^^^^^^^^^^^^^^
        (data ?? (true ? data : data)).FirstOrDefault(x => true); // Noncompliant
        //                             ^^^^^^^^^^^^^^
    }
}
