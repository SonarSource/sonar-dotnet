using System;

class foo          // Non-Compliant
{
}

class Foo          // Compliant
{
}

class I            // Non-Compliant
{
}

class IFoo         // Non-Compliant
{
}

class IdentityFoo  // Compliant
{
}

partial class Foo  // Compliant
{
}
