using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public abstract class IEnumerableReproducer
    {
        protected abstract IAsyncEnumerable<int> GetFilesAsync(string folderName); // Noncompliant - FP, async method (https://github.com/SonarSource/sonar-dotnet/issues/3433)
    }
}
