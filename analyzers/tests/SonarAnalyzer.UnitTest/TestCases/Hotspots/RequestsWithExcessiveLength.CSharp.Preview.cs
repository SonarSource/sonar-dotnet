using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.RequestsWithExcessiveLength.GenericAttributes;

internal class TestCases
{
    [RequestSizeLimit(8_000_001)] // Noncompliant
    public void Foo() { }

    [GenericAttribute<int>(8_000_001)] // FN
    public void Bar() { }
}

public class GenericAttribute<T> : RequestSizeLimitAttribute
{
    public GenericAttribute(long bytes) : base(bytes) { }
}
