#define BLOCK1
#define BLOCK2
#define BLOCK3

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class IfDirectiveTestHelper
    {
        [TestMethod]
        public void NoDirectives()
        {
            // Arrange
            var source = @"
namespace Test
{
  class TestClass
  {
    void Method1(){}
  }
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEmpty();
        }

        [TestMethod]
        public void ActiveBlocks_NonNestedIfs()
        {
            // Arrange
            var source = @"
#define BLOCK1
#define BLOCK2
#define BLOCK3

namespace Test
{
#if BLOCK1
#endif

#if BLOCK2
#endif

#if true // literal block
#endif

#if BLOCK3

    class TestClass
    {
        void Method1() { }
    }

#endif

#if BLOCK1
#endif

}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().ContainSingle();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK3" });
        }

        [TestMethod]
        public void ActiveBlocks_NestedIfs()
        {
            // Arrange
            var source = @"
#define BLOCK1
#define BLOCK2
#define BLOCK3
#define BLOCK4

namespace Test
{
#if BLOCK1
#if BLOCK2
#if BLOCK3
#if BLOCK4 // opened and closed, so should not appear
#endif

    class TestClass
    {
        void Method1() { }
    }

#endif
#endif
#endif

#if BLOCK1
#endif

}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().HaveCount(3);
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK1", "BLOCK2", "BLOCK3" });
        }

        [TestMethod]
        public void ActiveBlocks_DirectivesInLeadingTrivia()
        {
            // Arrange
            var source = @"
#define BLOCK1
#define BLOCK2
#define BLOCK3

namespace Test
{
    public class TestClass
    {
// trivia
#if BLOCK2
// more trivia
#if true // literal block
// more trivia
#endif
#if BLOCK3
// more trivia
        void Method1() { }
    }

#endif
#endif

#if BLOCK1
#endif

}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK2", "BLOCK3" });
        }

        [TestMethod]
        public void ActiveBlocks_ElseInPrecedingCode()
        {
            // Arrange
            var source = @"
#define BLOCK2

#if BLOCK1
#else

#if BLOCK2
#else
#elseif BLOCK3
#endif

#endif

#if BLOCK2
namespace Test
{
    class TestClass
    {
        void Method1() { }
    }
}
#endif
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK2" });
        }


        [TestMethod]
        public void ActiveBlocks_NegativeConditions_InIf()
        {
            // Arrange
            var source = @"
namespace Test
{
#if !BLOCK1
    class TestClass
    {
        void Method1() { }
    }
#else
#endif
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEmpty();
        }

        [TestMethod]
        public void ActiveBlocks_NegativeConditions_InElse()
        {
            // Arrange
            var source = @"
#define BLOCK1

namespace Test
{
#if !BLOCK1
#else
    class TestClass
    {
        void Method1() { }
    }
#endif
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEmpty();
        }

        [TestMethod]
        public void ActiveBlocks_Else_FirstBranchIsActive()
        {
            // Arrange
            var source = @"
#define BLOCK1

namespace Test
{
#if BLOCK1
    class TestClass
    {
        void Method1() { }
#else
    class TestClass
    {
        void Method1() {}
#endif
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK1" });
        }

        [TestMethod]
        public void ActiveBlocks_Else_SecondBranchIsActive()
        {
            // Arrange
            var source = @"
#define BLOCK2

namespace Test
{
#if BLOCK1
    class TestClass
    {
        void Method1() { }
#else
    class TestClass
    {
        void Method1() {}
#endif
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEmpty();
        }

        [TestMethod]
        public void ActiveBlocks_Elif_FirstBranchIsActive()
        {
            // Arrange
            var source = @"
#define BLOCK1
#define BLOCK2

namespace Test
{
#if BLOCK1
    class TestClass
    {
        void Method1() { }
#elif BLOCK2
    class TestClass
    {
        void Method1() {}
#endif
    }
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK1" });
        }

        [TestMethod]
        public void ActiveBlocks_Elif_SecondBranchIsActive()
        {
            // Arrange
            var source = @"
#define BLOCK2

namespace Test
{
#if BLOCK1
    class TestClass
    {
        void Method1() { }
#elif BLOCK2
    class TestClass
    {
        void Method1() {}
#endif
    }
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK2" });
        }

        [TestMethod]
        public void ActiveBlocks_Elif_FirstBranchIsActive_InLeadingTrivia()
        {
            // Arrange
            var source = @"
#define BLOCK1
#define BLOCK2

namespace Test
{
    class TestClass
    {
#if BLOCK1
        void Method1() { }
#elif BLOCK2
        void Method1() {}
#endif
    }
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK1" });
        }

        [TestMethod]
        public void ActiveBlocks_Elif_SecondBranchIsActive_InLeadingTrivia()
        {
            // Arrange
            var source = @"
#define BLOCK2

namespace Test
{
    class TestClass
    {
#if BLOCK1
        void Method1() { }
#elif BLOCK2
        void Method1() {}
#endif
    }
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK2" });
        }

        [TestMethod]
        public void InactiveDirectives_ShouldBeIgnored()
        {
            // Arrange
            var source = @"
#define BLOCK1
#define BLOCK2
#define BLOCK3
#define BLOCK4

namespace Test
{
#if INACTIVE1
#if BLOCK1  // inside inactive block -> ignored
#endif
#endif

#if BLOCK3
    class TestClass
    {
#if BLOCK4
#if INACTIVE2
#if BLOCK2  // inside inactive block -> ignored
#endif
#endif
        void Method1() { }
    }

#endif

#if BLOCK1
#endif

}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK3", "BLOCK4" });
        }

        [TestMethod]
        public void BadDirectives_ShouldBeIgnored()
        {
            // Arrange
            var source = @"
#define BLOCK2

#if BLOCK1
#endif
#else // bad directive

#endif // bad directive

#if BLOCK2
#FOO // bad directive
namespace Test
{
    class TestClass
    {
#BAR // bad directive
        void Method1() { }
    }
}
";
            var node = GetMethod1Node(source);

            // Act
            var activeSections = CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node);

            // Assert
            activeSections.Should().NotBeNull();
            activeSections.Should().BeEquivalentTo(new[] { "BLOCK2" });
        }

        private static SyntaxNode GetMethod1Node(string source) =>
            CSharpSyntaxTree.ParseText(source).GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "Method1");
    }
}

