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
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    using System.Linq;
    using System.Collections.Generic;
    public class D
    {
        public IList<string> Strings { get; set; }
    }

    public class C
    {
        public object Original(D d)
        {
            var a = d?.Strings;
            if (a != null)
            {
                return d.Strings.Select(s => a[0]);
            }
            return null;
        }
        public object EquivalentToOriginal(D d)
        {
            IList<string> a;
            if (d == null)
            {
                a = null;
            }
            else
            {
                a = d.Strings;
            }
            // since "a" is captured we don't store SV-Symbol relation and we don't "know" that
            // when "a != null" "d" is always not null.
            if (a != null)
            {
                return d.Strings.Select(s => a[0]);
            }
            return null;
        }
    }

    [TestClass]
    public class NullPointerDereferenceTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_Temp()
        {
            Verifier.VerifyCSharpAnalyzer(@"
    using System.Linq;
    using System.Collections.Generic;
    public class D
    {
        public IList<string> Strings { get; set; }
    }

    public class C
    {
        public object Original(D d)
        {
            var a = d?.Strings;
            if (a != null)
            {
                return d.Strings.Select(s => a[0]);
            }
            return null;
        }
    }
", new NullPointerDereference());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference()
        {
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereference.cs", new NullPointerDereference());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereferenceCSharp6()
        {
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereferenceCSharp6.cs", new NullPointerDereference(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }
    }
}
