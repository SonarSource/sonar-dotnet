using System;
using System.Linq;
using System.Collections.Generic;

class Sample
{
    // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/8428
    void CollectionExpressions(int[] array)
    {
        int[] knownLength = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        for (var i = 0; i < knownLength.Length; i++)
        {
            if (i > 5)                      // Compliant
            {
                Console.WriteLine(i);
            }
        }
        if (knownLength.Length == 0) { }    // FN
        if (knownLength.Length < 0) { }     // Noncompliant

        int[] knownLength2 = [1, 2, .. knownLength, 3, 4];
        for (var i = 0; i < knownLength.Length; i++)
        {
            if (i > 5)                      // Compliant
            {
                Console.WriteLine(i);
            }
        }
        if (knownLength2.Length == 0) { }   // FN
        if (knownLength2.Length < 0) { }    // Noncompliant

        int[] unknownLength = [1, 2, .. array, 3, 4];
        for (var i = 0; i < unknownLength.Length; i++)
        {
            if (i > 5)                      // Compliant
            {
                Console.WriteLine(i);
            }
        }
        if (unknownLength.Length == 0) { }  // FN
        if (unknownLength.Length < 0) { }   // Noncompliant

        int[] unknownLength2 = [.. array];
        for (var i = 0; i < unknownLength2.Length; i++)
        {
            if (i > 5)                      // Compliant
            {
                Console.WriteLine(i);
            }
        }
        if (unknownLength2.Length == 0) { } // Compliant
        if (unknownLength2.Length < 0) { }  // Noncompliant
    }

    // Repro for https://github.com/SonarSource/sonar-dotnet/issues/9671
    void ListFilledInLocalFunction()
    {
        List<int> list = new();
        foreach (var item in Enumerable.Range(0, 5))
        {
            if (item % 2 == 0)
            {
                LocalFunction(item);
            }

            void LocalFunction(int added)
            {
                list.Add(added);
            }
        }

        if (list.Count > 0) // Noncompliant FP
        {
            Console.WriteLine("This code is reachable"); // Secondary FP
        }
    }
}
