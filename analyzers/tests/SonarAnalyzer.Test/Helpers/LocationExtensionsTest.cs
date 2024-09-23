/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class LocationExtensionsTest
{
    [TestMethod]
    public void EnsureMappedLocation_NonGeneratedLocation_ShouldBeSame()
    {
        var code = """
            using System;

            namespace HelloWorld
            {
                class Program
                {
                    static void Main(string[] args)
                    {
                        Console.WriteLine("Hello, World!");
                    }
                }
            }
            """;
        var location = Location.Create(CSharpSyntaxTree.ParseText(code), TextSpan.FromBounds(50, 75));

        var result = location.EnsureMappedLocation();

        result.Should().BeSameAs(location);
    }

    [TestMethod]
    public void EnsureMappedLocation_LocationNull_ShouldReturnNull()
    {
        Location location = null;

        var result = location.EnsureMappedLocation();

        result.Should().BeNull();
    }

    [TestMethod]
    public void EnsureMappedLocation_GeneratedLocation_ShouldTargetOriginal()
    {
        var code = """
            using System;

            namespace HelloWorld
            {
                class Program
                {
                    static void Main(string[] args)
                    {
            #line (1, 5) - (1, 20) 30 "Original.razor"
                        Console.WriteLine("Hello, World!");
                    }
                }
            }
            """;
        var location = Location.Create(CSharpSyntaxTree.ParseText(code).WithFilePath("Program.razor.g.cs"), new TextSpan(code.IndexOf("\"Hello, World!\""), 15));

        var result = location.EnsureMappedLocation();

        result.Should().NotBeSameAs(location);
        result.GetLineSpan().Path.Should().Be("Original.razor");
        result.GetLineSpan().Span.Start.Line.Should().Be(0);
        result.GetLineSpan().Span.Start.Character.Should().Be(4);
        result.GetLineSpan().Span.End.Line.Should().Be(0);
        result.GetLineSpan().Span.End.Character.Should().Be(19);
    }
}
