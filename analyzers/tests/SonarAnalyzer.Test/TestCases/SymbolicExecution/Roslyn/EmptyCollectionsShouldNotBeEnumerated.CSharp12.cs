using System;
using System.Collections.Generic;

// https://github.com/SonarSource/sonar-dotnet/issues/8539
class CollectionExpressions
{
    void Method(int[] arrayArgument)
    {
        List<int> emptyList = [];
        emptyList.Clear();                  // FN

        ReadOnlySpan<int> emptySpan = [.. emptyList];
        foreach (int element in emptySpan)  // FN
        {
            Console.WriteLine(element);
        }

        HashSet<int> nonEmptyHashSet = [1, 2, 3];
        nonEmptyHashSet.Clear();            // Compliant

        HashSet<int> copiedNonEmptyHashSet = [.. nonEmptyHashSet];
        copiedNonEmptyHashSet.Clear();      // Compliant

        int[] copiedArrayArgument = [.. arrayArgument];
        copiedArrayArgument.GetValue(0);    // Compliant - the size of arrayArgument is not known

        List<int> extendedEmptyList = [.. emptyList, 42];
        extendedEmptyList.Clear();          // Compliant
    }
}
