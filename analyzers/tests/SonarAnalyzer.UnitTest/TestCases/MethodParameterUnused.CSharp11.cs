using System;

namespace SomeNamespace
{
    public class MethodParameterUnused
    {
        private void Argument_Unused(string argument) // Noncompliant
//                                   ^^^^^^^^^^^^^^^
        {
            var x = 42;
        }

        private void Argument_Reassigned(string argument) // Noncompliant
//                                       ^^^^^^^^^^^^^^^
        {
            argument = "So Long, and Thanks for All the Fish";
        }

        [Obsolete(nameof(argument))]
        private void Argument_UsedInAttributeByNameOf(string argument) // Compliant, because this rule ignores methods that have attributes, not because of the nameof.
        {
            var x = 42;
        }

        [Obsolete(nameof(TArgument))]
        private void Argument_UsedInGenericAttributeByNameOf<TArgument>(TArgument argument) // Compliant, because this rule ignores methods that have attributes, not because of the nameof.
        {
            var x = 42;
        }
    }
}

