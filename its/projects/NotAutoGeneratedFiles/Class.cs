using System;
using System.Diagnostics.CodeAnalysis;

namespace AAA
{
    class Class
    {
        [ExcludeFromCodeCoverage]
        private void Method(string[] args)
        {
            object obj = null;
            Console.WriteLine(obj.ToString());
        }
    }
}
