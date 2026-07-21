/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class OrderByRepeatedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<OrderByRepeated>();

    [TestMethod]
    public void OrderByRepeated() =>
        builder.AddPaths("OrderByRepeated.cs").Verify();

    [TestMethod]
    public void OrderByRepeated_Latest() =>
        builder.AddPaths("OrderByRepeated.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void OrderByRepeated_EntityFramework() =>
        builder.AddPaths("OrderByRepeated.EntityFramework.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore(TestConstants.NuGetLatestVersion).Concat(MetadataReferenceFacade.SystemComponentModelTypeConverter))
            .Verify();

    [TestMethod]
    [DynamicData(nameof(AzureCosmosSdks))]
    public void OrderByRepeated_AzureCosmosSdks(IEnumerable<MetadataReference> references, string sdkNamespace, string parameter, string orderedQueryableSource) =>
        builder
            .AddSnippet($$"""
                using System.Linq;
                using {{sdkNamespace}};

                public class Sample
                {
                    public int Id { get; set; }

                    public void Method({{parameter}})
                    {
                        {{orderedQueryableSource}}.OrderBy(x => x.Id);      // Compliant - returns IOrderedQueryable but does not actually order the sequence
                        new[] { 1, 2, 3 }.OrderBy(x => x).OrderBy(x => x);  // FN
                    }
                }
                """)
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(references)
            .VerifyNoIssues();

    [TestMethod]
    public void OrderByRepeated_CodeFix() =>
        builder
            .WithCodeFix<OrderByRepeatedCodeFix>()
            .AddPaths("OrderByRepeated.cs")
            .WithCodeFixedPaths("OrderByRepeated.Fixed.cs")
            .VerifyCodeFix();

    // These SDKs return IOrderedQueryables without actually ordering, so the rule is disabled for the whole compilation when the SDK is referenced.
    private static IEnumerable<(IEnumerable<MetadataReference> References, string SdkNamespace, string Parameter, string OrderedQueryableSource)> AzureCosmosSdks() =>
    [
        (NuGetMetadataReference.MicrosoftAzureCosmos(), "Microsoft.Azure.Cosmos", "Container container", "container.GetItemLinqQueryable<Sample>()"),
        (NuGetMetadataReference.MicrosoftAzureDocumentDB(), "Microsoft.Azure.Documents.Client", "DocumentClient client", "client.CreateDocumentQuery<Sample>(\"collectionLink\")"),
    ];
}
