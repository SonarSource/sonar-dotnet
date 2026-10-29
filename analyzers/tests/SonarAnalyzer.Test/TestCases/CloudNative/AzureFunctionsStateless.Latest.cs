using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

public static class AzureFunctionsStatic
{
    public static int Field;

    [FunctionName("Sample")]
    public static void SideEffects()
    {
        WithArg(Field >>>= 1); // Noncompliant
    }

    private static void WithArg(int value) { }
}

public static partial class StaticPartialClass
{
    public static partial int PartialProperty { get; set; }
    [FunctionName("Sample")]
    public static partial void Update(int value);
}

public static class ExtensionIndexers
{
    private static readonly List<int> Items = new();

    extension(List<int> list)
    {
        public int this[string key]
        {
            get => 0;
            set { }
        }
    }

    [FunctionName("Sample")]
    public static void Run()
    {
        Items["a"] = 1;     // Noncompliant
        var local = new List<int>();
        local["a"] = 1;     // Compliant
    }
}
