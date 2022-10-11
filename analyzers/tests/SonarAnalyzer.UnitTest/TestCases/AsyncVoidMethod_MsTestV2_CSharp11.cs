using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net6Poc.AsyncVoidMethod;

internal class MsTestCases
{
    [Generic<int>]
    public static async void M() { } // Noncompliant
}

public class GenericAttribute<T> : TestMethodAttribute { }
