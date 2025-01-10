/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Text;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class InfiniteRecursionTest
{
    private readonly VerifierBuilder sonarCfg = new VerifierBuilder()
        .AddAnalyzer(() => new InfiniteRecursion(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
        .AddReferences(MetadataReferenceFacade.NetStandard21);

    private readonly VerifierBuilder roslynCfg = new VerifierBuilder<InfiniteRecursion>()
        .AddReferences(MetadataReferenceFacade.NetStandard21);

    [TestMethod]
    public void InfiniteRecursion_SonarCfg() =>
        sonarCfg.AddPaths("InfiniteRecursion.SonarCfg.cs")
            .WithOptions(LanguageOptions.OnlyCSharp7)
            .Verify();

    [TestMethod]
    public void InfiniteRecursion_RoslynCfg() =>
        roslynCfg.AddPaths("InfiniteRecursion.RoslynCfg.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

#if NET

    [TestMethod]
    public void InfiniteRecursion_RoslynCfg_Latest() =>
        roslynCfg.AddPaths("InfiniteRecursion.RoslynCfg.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

    // https://github.com/SonarSource/sonar-dotnet/issues/8977
    [TestMethod]
    public void InfiniteRecursion_RoslynCfg_8977()
    {
        const int rows = 4_000;
        var code = new StringBuilder();
        code.Append("""
            using UInt32Value = System.UInt32;
            using StringValue = System.String;

            public class WorksheetPart
            {
                public Worksheet Worksheet { get; set; }
            }
            public class Worksheet
            {
                public MarkupCompatibilityAttributes MCAttributes { get; set; }
                public void AddNamespaceDeclaration(string alias, string xmlNamespace) { }
                public void Append(SheetData sheetData1) { }
            }
            public class SheetData
            {
                public void Append(Row r) { }
            }
            public class MarkupCompatibilityAttributes
            {
                public string Ignorable { get; set; }
            }
            public class Row
            {
                public UInt32Value RowIndex { get; set; }
                public ListValue<StringValue> Spans { get; set; }
                public double DyDescent { get; set; }
                public void Append(Cell c) { }
            }
            public class ListValue<T>
            {
                public string InnerText { get; set; }
            }
            public class Cell
            {
                public string CellReference { get; set; }
                public UInt32Value StyleIndex { get; set; }
                public void Append(CellValue c) { }
            }
            public class CellValue
            {
                public string Text { get; set; }
            }

            class Program
            {
                public static void Main() { }

                void GenerateWorksheetPart1Content(WorksheetPart worksheetPart1)
                {
                    Worksheet worksheet1 = new Worksheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac xr xr2 xr3" } };
                    worksheet1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                    worksheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
                    worksheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
                    worksheet1.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
                    worksheet1.AddNamespaceDeclaration("xr2", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
                    worksheet1.AddNamespaceDeclaration("xr3", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3");

                    SheetData sheetData1 = new SheetData();
            """);
        for (var i = 1; i <= rows; i++)
        {
            code.Append($$"""
                Row row{{i}} = new Row() { RowIndex = (UInt32Value)1U, Spans = new ListValue<StringValue>() { InnerText = "1:1" }, DyDescent = 0.25D };

                Cell cell{{i}} = new Cell() { CellReference = "A{{i}}", StyleIndex = (UInt32Value)1U };
                CellValue cellValue{{i}} = new CellValue();
                cellValue{{i}}.Text = "{{i}}";

                cell{{i}}.Append(cellValue{{i}});

                row{{i}}.Append(cell{{i}});
                """);
        }
        for (var i = 1; i <= rows; i++)
        {
            code.AppendLine($$"""        sheetData1.Append(row{{i}});""");
        }
        code.Append(""""
                worksheet1.Append(sheetData1);
                worksheetPart1.Worksheet = worksheet1;
            }
        }
        """");

        roslynCfg.AddSnippet(code.ToString())
            .WithOptions(LanguageOptions.FromCSharp8)
            .VerifyNoIssues();
    }
}
