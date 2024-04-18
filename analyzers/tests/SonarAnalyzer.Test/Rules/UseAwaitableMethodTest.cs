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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseAwaitableMethodTest
{
    private const string EntityFrameworkVersion = "7.0.18";

    private readonly VerifierBuilder builder = new VerifierBuilder<UseAwaitableMethod>();

    [TestMethod]
    public void UseAwaitableMethod_CS() =>
        builder.AddPaths("UseAwaitableMethod.cs").Verify();

    [TestMethod]
    public void UseAwaitableMethod_CS_Test() =>
        builder
        .WithOptions(ParseOptionsHelper.FromCSharp11)
        .AddReferences(MetadataReferenceFacade.SystemNetPrimitives)
        .AddReferences(MetadataReferenceFacade.SystemNetSockets)
        .AddSnippet("""
            using System;
            using System.Text;
            using System.IO;
            using System.Net;
            using System.Net.Sockets;
            using System.Threading.Tasks;
            using System.Collections.Generic;

            public class Sockets
            {
                async Task<Action> CreateActionAsync(StreamReader reader)
                {
                    Action action = () =>
                    {
                        reader.ReadLine();      // Compliant
                    };
                    return action;
                }
            }                        
            """).Verify();

    [TestMethod]
    public void UseAwaitableMethod_CSharp9() =>
        builder
        .WithTopLevelStatements()
        .AddPaths("UseAwaitableMethod_CSharp9.cs")
        .Verify();

    [TestMethod]
    public void UseAwaitableMethod_CSharp8() =>
        builder
        .WithOptions(ParseOptionsHelper.FromCSharp8)
        .AddPaths("UseAwaitableMethod_CSharp8.cs")
        .Verify();

#if NET
    [TestMethod]
    public void UseAwaitableMethod_EF() =>
        builder
        .WithOptions(ParseOptionsHelper.FromCSharp11)
        .AddReferences([CoreMetadataReference.SystemComponentModelTypeConverter])
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore(EntityFrameworkVersion))
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(EntityFrameworkVersion))
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(EntityFrameworkVersion))
        .AddPaths("UseAwaitableMethod_EF.cs")
        .Verify();
#endif

    [TestMethod]
    public void UseAwaitableMethod_Sockets() =>
        builder
        .AddReferences(MetadataReferenceFacade.SystemNetPrimitives)
        .AddReferences(MetadataReferenceFacade.SystemNetSockets)
        .AddPaths("UseAwaitableMethod_Sockets.cs")
        .Verify();
}
