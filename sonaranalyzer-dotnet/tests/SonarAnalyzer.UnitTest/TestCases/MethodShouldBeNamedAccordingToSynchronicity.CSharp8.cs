using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public abstract class IEnumerableReproducer
    {
        protected abstract IAsyncEnumerable<int> GetFilesAsync(string folderName); // Compliant (https://github.com/SonarSource/sonar-dotnet/issues/3433)

        protected abstract IAsyncEnumerable<int> GetFiles(string folderName); // Noncompliant
    }
}
