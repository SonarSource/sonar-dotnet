using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public abstract class IEnumerableReproducer
    {
        protected abstract IAsyncEnumerable<int> GetFilesAsync(string folderName); // Compliant (https://github.com/SonarSource/sonar-dotnet/issues/3433)

        protected abstract IAsyncEnumerable<int> GetFiles(string folderName); // Noncompliant

        protected abstract CustomAsyncEnumerable<int> GetFilesCustomAsyncEnumerableAsync(string folderName); // Compliant

        protected abstract CustomAsyncEnumerable<int> GetFilesCustomAsyncEnumerable(string folderName); // Noncompliant

        protected abstract CustomTask<int> GetFilesCustomTaskAsync(string folderName); // Compliant

        protected abstract CustomTask<int> GetFilesCustomTask(string folderName); // Noncompliant

        protected abstract void NoReturnType(string folderName);
    }

    public class CustomAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancel = new CancellationToken())
            => throw new System.NotImplementedException();
    }

    public class CustomTask<T> : Task<T>
    {
        public CustomTask(Func<T> function) : base(function)
        {
        }
    }
}
