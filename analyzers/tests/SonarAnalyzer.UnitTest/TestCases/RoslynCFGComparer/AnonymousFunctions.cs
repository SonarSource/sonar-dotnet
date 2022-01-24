using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.UnitTest.TestCases.RoslynCFGComparer
{
    public class AnonymousFunctions
    {
        public IEnumerable<int> SimpleCall() => new[] {1, 2}.OrderBy(x => x);
    }
}
