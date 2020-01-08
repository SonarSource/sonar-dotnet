using System.Diagnostics.CodeAnalysis;
using System.IO;
using CBDEArguments;

namespace CBDESucceedsWithIncorrectResults
{
    [ExcludeFromCodeCoverage]
    static class Program
    {
        static void Main(string[] args)
        {
            var output = CbdeArguments.GetOutputPath(args);
            File.WriteAllText(output, "This is not valid XML");
        }
    }
}
