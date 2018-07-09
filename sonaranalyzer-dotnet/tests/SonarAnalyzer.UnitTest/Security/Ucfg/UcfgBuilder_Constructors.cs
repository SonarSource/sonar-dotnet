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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Security.Framework;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class UcfgBuilder_Constructors
    {
        [TestMethod]
        public void Ctor_Calling_Implicit()
        {
            var code = @"
class Foo
{
    public Foo(string s)
    {
                                    // no parameterless ctor called here yet
        var x = s;                  // x := __id [ s ]
    }

    public Foo() {}
}
";
            UcfgVerifier.VerifyInstructions(code, "Foo", isCtor: true);
        }

        [TestMethod]
        public void Ctor_FieldInitializers()
        {
            var code = @"
class Foo
{
    string field = 5.ToString();
    public Foo(string s)
    {
                                    // no implicit ctor with initializers called here yet
        var x = s;                  // x := __id [ s ]
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "Foo", isCtor: true);
        }

        [TestMethod]
        public void Ctor_Calling_This()
        {
            var code = @"
class Foo
{
    // The ctor initializer instructions are executed before the ctor body

    public Foo() : this(""a"" + 5.ToString())       // %0 := int.ToString() [ const ]
                                                    // %1 := __concat [ %0 const ]
                                                    // %2 := Foo.Foo(string) [ this %1 ]
    {
        var x = s;                                  // x := __id [ const ]
    }

    public Foo(string s) {}
}
";
            UcfgVerifier.VerifyInstructions(code, "Foo", isCtor: true);
        }

        [TestMethod]
        public void Ctor_Calling_Base()
        {
            var code = @"
class Foo : Bar
{
    // The ctor initializer instructions are executed before the ctor body

    public Foo() : base(""a"" + 5.ToString())       // %0 := int.ToString() [ const ]
                                                    // %1 := __concat [ %0 const ]
                                                    // %2 := Bar.Bar(string) [ this %1 ]
    {
        var x = 5;                                  // x := __id [ const ]
    }
}
class Bar
{
    public Bar(string s) {}
}
";
            UcfgVerifier.VerifyInstructions(code, "Foo", isCtor: true);
        }
    }
}
