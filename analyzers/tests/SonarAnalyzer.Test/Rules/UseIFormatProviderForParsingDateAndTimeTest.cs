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

using System.Globalization;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseIFormatProviderForParsingDateAndTimeTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseIFormatProviderForParsingDateAndTime>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseIFormatProviderForParsingDateAndTime>();

    [TestMethod]
    public void UseIFormatProviderForParsingDateAndTime_CS() =>
        builderCS.AddPaths("UseIFormatProviderForParsingDateAndTime.cs").Verify();

    [TestMethod]
    public void UseIFormatProviderForParsingDateAndTime_VB() =>
        builderVB.AddPaths("UseIFormatProviderForParsingDateAndTime.vb").Verify();

#if NET

    [TestMethod]
    [DataRow(nameof(DateTime), nameof(DateTimeStyles))]
    [DataRow(nameof(DateTimeOffset), nameof(DateTimeStyles))]
    [DataRow(nameof(TimeSpan), nameof(TimeSpanStyles))]
    [DataRow(nameof(DateOnly), nameof(DateTimeStyles))]
    [DataRow(nameof(TimeOnly), nameof(DateTimeStyles))]
    public void UseIFormatProviderForParsingDateAndTime__MethodOverloads_CS(string temporalTypeName, string styleTypeName) =>
        builderCS.AddSnippet($$$"""
using System;
using System.Globalization;

class Test
{
    void ParseOverloads()
    {
        {{{temporalTypeName}}}.Parse("01/02/2000");                                           // Noncompliant

        {{{temporalTypeName}}}.Parse("01/02/2000", null);                                     // Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000", CultureInfo.InvariantCulture);             // Compliant

        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), null);                            // Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture);    // Compliant

        {{{temporalTypeName}}}.Parse("01/02/2000", null);                                     // Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000", CultureInfo.InvariantCulture);             // Compliant

        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), null);                            // Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture);    // Compliant
    }

    void ParseExactOverloads()
    {
        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", null);                                                                           // Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture);                                                   // Compliant

        {{{temporalTypeName}}}.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), null);                                                         // Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), CultureInfo.InvariantCulture);                                 // Compliant

        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", null, {{{styleTypeName}}}.None);                                                 // Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, {{{styleTypeName}}}.None);                         // Compliant

        {{{temporalTypeName}}}.ParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, null, {{{styleTypeName}}}.None);                         // Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, CultureInfo.InvariantCulture, {{{styleTypeName}}}.None); // Compliant
    }

    void TryParseOverloads()
    {
        {{{temporalTypeName}}} parsedDate;

        {{{temporalTypeName}}}.TryParse("01/02/2000", out parsedDate);                                          // Noncompliant
        {{{temporalTypeName}}}.TryParse("01/02/2000".AsSpan(), out parsedDate);                                 // Noncompliant

        {{{temporalTypeName}}}.TryParse("01/02/2000", null, out parsedDate);                                    // Noncompliant
        {{{temporalTypeName}}}.TryParse("01/02/2000", CultureInfo.InvariantCulture, out parsedDate);            // Compliant

        {{{temporalTypeName}}}.TryParse("01/02/2000".AsSpan(), null, out parsedDate);                           // Noncompliant
        {{{temporalTypeName}}}.TryParse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture, out parsedDate);   // Compliant
    }

    void TryParseExactOverloads()
    {
        {{{temporalTypeName}}} parsedDate;

        {{{temporalTypeName}}}.TryParseExact("01/02/2000", "dd/MM/yyyy", null, {{{styleTypeName}}}.None, out parsedDate);                                                              // Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, out parsedDate);                                      // Compliant

        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", null, {{{styleTypeName}}}.None, out parsedDate);                                                     // Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, out parsedDate);                             // Compliant

        {{{temporalTypeName}}}.TryParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, null, {{{styleTypeName}}}.None, out parsedDate);                                      // Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, out parsedDate);              // Compliant

        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), new[] { "dd/MM/yyyy", "dd MM yyyy" }, null, {{{styleTypeName}}}.None, out parsedDate);                             // Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), new[] { "dd/MM/yyyy", "dd MM yyyy" }, CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, out parsedDate);     // Compliant
    }
}
""").Verify();

    [TestMethod]
    [DataRow(nameof(DateTime), nameof(DateTimeStyles))]
    [DataRow(nameof(DateTimeOffset), nameof(DateTimeStyles))]
    [DataRow(nameof(TimeSpan), nameof(TimeSpanStyles))]
    [DataRow(nameof(DateOnly), nameof(DateTimeStyles))]
    [DataRow(nameof(TimeOnly), nameof(DateTimeStyles))]
    public void UseIFormatProviderForParsingDateAndTime__MethodOverloads_VB(string temporalTypeName, string styleTypeName) =>
        builderVB.AddSnippet($$$"""
Imports System.Globalization

Class Test
    Sub ParseOverloads()
        {{{temporalTypeName}}}.Parse("01/02/2000")                                           ' Noncompliant

        {{{temporalTypeName}}}.Parse("01/02/2000", Nothing)                                  ' Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000", CultureInfo.InvariantCulture)             ' Compliant

        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), Nothing)                         ' Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture)    ' Compliant

        {{{temporalTypeName}}}.Parse("01/02/2000", Nothing)                                  ' Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000", CultureInfo.InvariantCulture)             ' Compliant

        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), Nothing)                         ' Noncompliant
        {{{temporalTypeName}}}.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture)    ' Compliant
    End Sub

    Sub ParseExactOverloads()
        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", Nothing)                                                                ' Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture)                                           ' Compliant

        {{{temporalTypeName}}}.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), Nothing)                                              ' Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), CultureInfo.InvariantCulture)                         ' Compliant

        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", Nothing, {{{styleTypeName}}}.None)                                      ' Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, {{{styleTypeName}}}.None)                 ' Compliant

        {{{temporalTypeName}}}.ParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, Nothing, {{{styleTypeName}}}.None)                      ' Noncompliant
        {{{temporalTypeName}}}.ParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, CultureInfo.InvariantCulture, {{{styleTypeName}}}.None) ' Compliant
    End Sub

    Sub TryParseOverloads()
        Dim parsedDate As {{{temporalTypeName}}}

        {{{temporalTypeName}}}.TryParse("01/02/2000", parsedDate)                                        ' Noncompliant
        {{{temporalTypeName}}}.TryParse("01/02/2000".AsSpan(), parsedDate)                               ' Noncompliant

        {{{temporalTypeName}}}.TryParse("01/02/2000", Nothing, parsedDate)                               ' Noncompliant
        {{{temporalTypeName}}}.TryParse("01/02/2000", CultureInfo.InvariantCulture, parsedDate)          ' Compliant

        {{{temporalTypeName}}}.TryParse("01/02/2000".AsSpan(), Nothing, parsedDate)                      ' Noncompliant
        {{{temporalTypeName}}}.TryParse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture, parsedDate) ' Compliant
    End Sub

    Sub TryParseExactOverloads()
        Dim parsedDate As {{{temporalTypeName}}}

        {{{temporalTypeName}}}.TryParseExact("01/02/2000", "dd/MM/yyyy", Nothing, {{{styleTypeName}}}.None, parsedDate)                                                ' Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, parsedDate)                           ' Compliant

        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", Nothing, {{{styleTypeName}}}.None, parsedDate)                                       ' Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, parsedDate)                  ' Compliant

        {{{temporalTypeName}}}.TryParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, Nothing, {{{styleTypeName}}}.None, parsedDate)                                ' Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, parsedDate)           ' Compliant

        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), {"dd/MM/yyyy", "dd MM yyyy"}, Nothing, {{{styleTypeName}}}.None, parsedDate)                       ' Noncompliant
        {{{temporalTypeName}}}.TryParseExact("01/02/2000".AsSpan(), {"dd/MM/yyyy", "dd MM yyyy"}, CultureInfo.InvariantCulture, {{{styleTypeName}}}.None, parsedDate)  ' Compliant
    End Sub
End Class
""").Verify();

#endif

}
