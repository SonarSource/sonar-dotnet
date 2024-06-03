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

            public class C
            {
                public Action ActionProperty { get; }
                public Func<Task> ActionPropertyAsync { get; }

                async Task MethodInvocations()
                {
                    ActionProperty(); // Compliant;
                }
            }
            """).VerifyNoIssues();

    [TestMethod]
    public void UseAwaitableMethod_Sockets() =>
        builder
        .AddReferences(MetadataReferenceFacade.SystemNetPrimitives)
        .AddReferences(MetadataReferenceFacade.SystemNetSockets)
        .AddPaths("UseAwaitableMethod_Sockets.cs")
        .Verify();

    [TestMethod]
    public void UseAwaitableMethod_FluentValidation() =>
        builder
        .AddReferences(NuGetMetadataReference.FluentValidation())
        .AddPaths("UseAwaitableMethod_FluentValidation.cs")
        .Verify();

    [TestMethod]
    public void UseAwaitableMethod_ExcludeXmlReaderAndWriter() =>
        builder
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .AddSnippet("""
                using System.IO;
                using System.Threading.Tasks;
                using System.Xml;

                public class Test
                {
                    async Task TestReader(Stream stream)
                    {
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            reader.Read();                           // Compliant, we don't raise for XmlReader methods https://github.com/SonarSource/sonar-dotnet/issues/9336
                            reader.ReadContentAs(typeof(int), null); // Compliant
                            reader.MoveToContent();                  // Compliant
                            reader.ReadContentAsBase64(null, 0, 0);  // Compliant
                            reader.ReadContentAsBinHex(null, 0, 0);  // Compliant
                            reader.ReadContentAsObject();            // Compliant
                            reader.ReadContentAsString();            // Compliant
                            reader.ReadInnerXml();                   // Compliant
                            reader.ReadOuterXml();                   // Compliant
                            reader.ReadValueChunk(null, 0, 0);       // Compliant
                        }

                        using (XmlWriter writer = XmlWriter.Create(stream))
                        {
                            writer.WriteStartElement("pf", "root", "http://ns");    // Compliant, we don't raise for XmlWriter methods https://github.com/SonarSource/sonar-dotnet/issues/9336
                            writer.WriteStartElement(null, "sub", null);            // Compliant
                            writer.WriteAttributeString(null, "att", null, "val");  // Compliant
                            writer.WriteString("text");                             // Compliant
                            writer.WriteEndElement();                               // Compliant
                            writer.WriteProcessingInstruction("pName", "pValue");   // Compliant
                            writer.WriteComment("cValue");                          // Compliant
                            writer.WriteCData("cdata value");                       // Compliant
                            writer.WriteEndElement();                               // Compliant
                            writer.Flush();                                         // Compliant
                        }
                    }
                }
            """).VerifyNoIssues();

#if NET
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
        .AddReferences([CoreMetadataReference.SystemComponentModelTypeConverter, CoreMetadataReference.SystemDataCommon])
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore(EntityFrameworkVersion))
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(EntityFrameworkVersion))
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(EntityFrameworkVersion))
        .AddPaths("UseAwaitableMethod_EF.cs")
        .Verify();

    [TestMethod]
    public void UseAwaitableMethod_MongoDb() =>
        builder
        .WithOptions(ParseOptionsHelper.FromCSharp11)
        .AddReferences(NuGetMetadataReference.MongoDBDriver())
        .AddPaths("UseAwaitableMethod_MongoDBDriver.cs")
        .VerifyNoIssues();

    [TestMethod]
    public void UseAwaitableMethod_Fix() =>
        builder
        .WithOptions(ParseOptionsHelper.FromCSharp11)
        .AddReferences(NuGetMetadataReference.MongoDBDriver())
        .AddPaths("UseAwaitableMethod_MongoDBDriver.cs")
        .VerifyNoIssues();
#endif
}
