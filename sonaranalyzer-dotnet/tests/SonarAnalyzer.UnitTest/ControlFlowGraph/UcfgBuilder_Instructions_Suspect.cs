/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    [TestClass]
    public class UcfgBuilder_Instructions_Suspect
    {
        [TestMethod]
        public void IndexerAccess()
        {
            const string code = @"
namespace Namespace
{
    using System.Collections.Generic;
    public class Class1
    {
        public void Foo(string s, List<string> list)
        {
            // Strings and lists have indexers but should not be handled as arrays;
            // until collection indexers are supported, the string and list element
            // access is represented as variable
            var c = s[0];
                // c := __id [ const ]
                /* Expect to have:
                 * %0 := string.this[int].get [ s const ]
                 * c := __id [ %0 ]
                 */

            var i = list[0];
                // i := __id [ const ]
                /* Expect to have:
                 * %1 := System.Collections.Generic.List<T>.this[int].get [ list const ]
                 * i := __id [ %1 ]
                 */
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void TernaryOperator_NotInCFG()
        {
            const string code = @"
using System;

public class Foo
{
    public void Bar(bool b)
    {
        // The ternary operator is not walked because it is a Jump node of a block
        // that's why when we create instruction for the variable declarator we
        // get NRE for the assignment argument.
        var s = b ? ""s1"" : ""s2"";
            // s := __id [ {unknown} ]
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Bar");
        }
    }
}
