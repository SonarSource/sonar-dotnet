using System.IO;
using System.Threading;
using CBDEArguments;

namespace CBDEWaitAndSucceed
{
    static class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(10_000);
            var output = CbdeArguments.GetOutputPath(args);
            File.WriteAllText(output, @"<?xml version=""1.0""?>
<Issues />
");
        }
    }
}
