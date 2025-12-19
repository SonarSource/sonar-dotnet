using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

internal class UseIsExtension
{
    public void Test(IMethodSymbol methodSymbol)
    {
        _ = methodSymbol.IsExtensionMethod;                                 // Noncompliant {{Use 'IsExtension' instead of 'IsExtensionMethod'. It also covers extension methods defined in extension blocks.}}
        IsExtensionMethod();                                                // Compliant
        _ = new Inner().IsExtensionMethod;                                  // Compliant
        _ = methodSymbol is { IsExtensionMethod: true };                    // Noncompliant
        _ = methodSymbol is { OriginalDefinition.IsExtensionMethod: true }; // Noncompliant
        //                                       ^^^^^^^^^^^^^^^^^
        _ = methodSymbol is { OriginalDefinition.IsExtension: true };       // Compliant
    }

    private void IsExtensionMethod()
    {
        throw new NotImplementedException();
    }

    class Inner
    {
        public bool IsExtensionMethod => true;
    }
}

public static class IMethodSymbolExtensions
{
    extension(IMethodSymbol methodSymbol)
    {
        public bool IsExtension => true;
    }
}
