using System.IO;
using CBDEArguments;

namespace CBDESucceedsWithIncorrectResults
{
    static class Program
    {
        static void Main(string[] args)
        {
            var output = CbdeArguments.GetOutputPath(args);
            File.WriteAllText(output, "This is not valid XML");
        }
    }
}
