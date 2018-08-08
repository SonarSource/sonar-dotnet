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
    public partial class UcfgInstructionTest
    {
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
            // s := __id [ const ]
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Bar");
        }

        [TestMethod]
        public void Dynamic()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string Foo(Class1 c)
        {
            dynamic dyn = c.Bar;
                // dyn := __id [ const ]

            if (dyn.User != null)
                // %0 := dynamic.operator !=(dynamic, dynamic) [ __unknown const const ]
            {
                return ""bar"";
            }

            return c.ToString();
                // %1 := object.ToString() [ c ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void NullCoalescingOperator()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        private Class1 field;
         public void Foo(Class1 c)
        {
            var result = c ?? field;
                // %0 := __id [ this.field ]
                // result := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Peach_Exception_InvalidTarget_Enum()
        {
            const string code = @"
// Adapted from https://github.com/Microsoft/automatic-graph-layout/blob/ba8a85105b8a420762b53fb0c8513b7f10fb30d9/GraphLayout/MSAGL/Layout/Incremental/Multipole/KDTree.cs
namespace Microsoft.Msagl.Layout.Incremental
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
 
    public class KDTree
    {
        Particle[] particles;

        private Particle[] particlesBy(Particle.Dim d)
        {
            return null;
        }

        public void Constructor(Particle[] particles, int bucketSize)
        {
            this.particles = particles;
            Particle[][] ps = new Particle[][] {
                particlesBy(Particle.Dim.Horizontal),
                particlesBy(Particle.Dim.Vertical)};
        }
    }

    public class Particle
    {
        internal enum Dim { Horizontal = 0, Vertical = 1 };
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "Constructor");
        }
    }

}
