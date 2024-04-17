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

#if NET

    [TestMethod]
    public void UseAwaitableMethod_CS_Test() =>
        builder
        .WithOptions(ParseOptionsHelper.FromCSharp11)
        .AddReferences([CoreMetadataReference.SystemComponentModelTypeConverter])
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore(EntityFrameworkVersion))
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(EntityFrameworkVersion))
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(EntityFrameworkVersion))
        .AddSnippet("""
            using Microsoft.EntityFrameworkCore;
            using System.IO;
            using System.Threading.Tasks;
            using System;
            using System.Linq;

            public class C
            {
                public C Child { get; }
                void VoidMethod() { }
                Task VoidMethodAsync() => Task.CompletedTask;

                C ReturnMethod() => null;
                Task<C> ReturnMethodAsync() => Task.FromResult<C>(null);

                bool BoolMethod() => true;
                Task<bool> BoolMethodAsync() => Task.FromResult(true);

                C this[int i] => null;
                public static C operator +(C c) => default(C);
                public static C operator +(C c1, C c2) => default(C);
                public static C operator -(C c) => default(C);
                public static C operator -(C c1, C c2) => default(C);
                public static C operator !(C c) => default(C);
                public static C operator ~(C c) => default(C);
                public static implicit operator int(C c) => default(C);

                async Task MethodInvocations()
                {
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
}
