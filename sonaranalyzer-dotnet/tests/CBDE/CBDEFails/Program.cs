using System;
using System.Diagnostics.CodeAnalysis;

namespace CBDEFails
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            Environment.Exit(-42);
        }
    }
}
