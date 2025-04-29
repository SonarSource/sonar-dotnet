using System;
using System.Collections;
using System.Collections.Generic;

/*
 * DO NOT ADD ANYTHING ELSE HERE
 */

namespace Tests.Diagnostics
{
    public interface IParentInAnotherFile : IAnotherFile, IAnotherFile2 // Noncompliant {{Rename or add member 'Method(int)' to this interface to resolve ambiguities.}}
    {
    }
}
