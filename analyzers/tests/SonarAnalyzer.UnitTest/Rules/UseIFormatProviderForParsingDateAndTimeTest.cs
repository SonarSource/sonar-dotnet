/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules;

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
    public void UseIFormatProviderForParsingDateAndTime__MethodOverloads_CS() =>
        builderCS.AddSnippet("""
using System;
using System.Globalization;

class Test
{
    void ParseOverloads()
    {
        DateTime.Parse("01/02/2000");                                           // Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.Parse' method.}}

        DateTime.Parse("01/02/2000", null);                                     // Noncompliant
        DateTime.Parse("01/02/2000", CultureInfo.InvariantCulture);             // Compliant

        DateTime.Parse("01/02/2000".AsSpan(), null);                            // Noncompliant
        DateTime.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture);    // Compliant

        DateTime.Parse("01/02/2000", null);                                     // Noncompliant
        DateTime.Parse("01/02/2000", CultureInfo.InvariantCulture);             // Compliant

        DateTime.Parse("01/02/2000".AsSpan(), null);                            // Noncompliant
        DateTime.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture);    // Compliant
    }

    void ParseExactOverloads()
    {
        DateTime.ParseExact("01/02/2000", "dd/MM/yyyy", null);                                                                      // Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.ParseExact' method.}}
        DateTime.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture);                                              // Compliant

        DateTime.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), null);                                                    // Noncompliant
        DateTime.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), CultureInfo.InvariantCulture);                            // Compliant

        DateTime.ParseExact("01/02/2000", "dd/MM/yyyy", null, DateTimeStyles.None);                                                 // Noncompliant
        DateTime.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);                         // Compliant

        DateTime.ParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, null, DateTimeStyles.None);                         // Noncompliant
        DateTime.ParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None); // Compliant
    }

    void TryParseOverloads()
    {
        DateTime parsedDate;

        DateTime.TryParse("01/02/2000", out parsedDate);                                                                // Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.TryParse' method.}}
        DateTime.TryParse("01/02/2000".AsSpan(), out parsedDate);                                                       // Noncompliant

        DateTime.TryParse("01/02/2000", null, out parsedDate);                                                          // Noncompliant
        DateTime.TryParse("01/02/2000", CultureInfo.InvariantCulture, out parsedDate);                                  // Compliant

        DateTime.TryParse("01/02/2000".AsSpan(), null, out parsedDate);                                                 // Noncompliant
        DateTime.TryParse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture, out parsedDate);                         // Compliant
    }

    void TryParseExactOverloads()
    {
        DateTime parsedDate;

        DateTime.TryParseExact("01/02/2000", "dd/MM/yyyy", null, DateTimeStyles.None, out parsedDate);                                                              // Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.TryParseExact' method.}}
        DateTime.TryParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);                                      // Compliant

        DateTime.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", null, DateTimeStyles.None, out parsedDate);                                                     // Noncompliant
        DateTime.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);                             // Compliant

        DateTime.TryParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, null, DateTimeStyles.None, out parsedDate);                                      // Noncompliant
        DateTime.TryParseExact("01/02/2000", new[] { "dd/MM/yyyy", "dd MM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);              // Compliant

        DateTime.TryParseExact("01/02/2000".AsSpan(), new[] { "dd/MM/yyyy", "dd MM yyyy" }, null, DateTimeStyles.None, out parsedDate);                             // Noncompliant
        DateTime.TryParseExact("01/02/2000".AsSpan(), new[] { "dd/MM/yyyy", "dd MM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);     // Compliant
    }
}
""").Verify();

    [TestMethod]
    public void UseIFormatProviderForParsingDateAndTime__MethodOverloads_VB() =>
        builderVB.AddSnippet("""
Imports System.Globalization

Class Test
    Sub ParseOverloads()
        Date.Parse("01/02/2000")                                           ' Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.Parse' method.}}

        Date.Parse("01/02/2000", Nothing)                                  ' Noncompliant
        Date.Parse("01/02/2000", CultureInfo.InvariantCulture)             ' Compliant

        Date.Parse("01/02/2000".AsSpan(), Nothing)                         ' Noncompliant
        Date.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture)    ' Compliant

        Date.Parse("01/02/2000", Nothing)                                  ' Noncompliant
        Date.Parse("01/02/2000", CultureInfo.InvariantCulture)             ' Compliant

        Date.Parse("01/02/2000".AsSpan(), Nothing)                         ' Noncompliant
        Date.Parse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture)    ' Compliant
    End Sub

    Sub ParseExactOverloads()
        Date.ParseExact("01/02/2000", "dd/MM/yyyy", Nothing)                                                           ' Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.ParseExact' method.}}
        Date.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture)                                      ' Compliant

        Date.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), Nothing)                                         ' Noncompliant
        Date.ParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy".AsSpan(), CultureInfo.InvariantCulture)                    ' Compliant

        Date.ParseExact("01/02/2000", "dd/MM/yyyy", Nothing, DateTimeStyles.None)                                      ' Noncompliant
        Date.ParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None)                 ' Compliant

        Date.ParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, Nothing, DateTimeStyles.None)                      ' Noncompliant
        Date.ParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, CultureInfo.InvariantCulture, DateTimeStyles.None) ' Compliant
    End Sub

    Sub TryParseOverloads()
        Dim parsedDate As Date

        Date.TryParse("01/02/2000", parsedDate)                                        ' Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.TryParse' method.}}
        Date.TryParse("01/02/2000".AsSpan(), parsedDate)                               ' Noncompliant

        Date.TryParse("01/02/2000", Nothing, parsedDate)                               ' Noncompliant
        Date.TryParse("01/02/2000", CultureInfo.InvariantCulture, parsedDate)          ' Compliant

        Date.TryParse("01/02/2000".AsSpan(), Nothing, parsedDate)                      ' Noncompliant
        Date.TryParse("01/02/2000".AsSpan(), CultureInfo.InvariantCulture, parsedDate) ' Compliant
    End Sub

    Sub TryParseExactOverloads()
        Dim parsedDate As Date

        Date.TryParseExact("01/02/2000", "dd/MM/yyyy", Nothing, DateTimeStyles.None, parsedDate)                                                ' Noncompliant {{Pass an 'IFormatProvider' to the 'DateTime.TryParseExact' method.}}
        Date.TryParseExact("01/02/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate)                           ' Compliant

        Date.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", Nothing, DateTimeStyles.None, parsedDate)                                       ' Noncompliant
        Date.TryParseExact("01/02/2000".AsSpan(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate)                  ' Compliant

        Date.TryParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, Nothing, DateTimeStyles.None, parsedDate)                                ' Noncompliant
        Date.TryParseExact("01/02/2000", {"dd/MM/yyyy", "dd MM yyyy"}, CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate)           ' Compliant

        Date.TryParseExact("01/02/2000".AsSpan(), {"dd/MM/yyyy", "dd MM yyyy"}, Nothing, DateTimeStyles.None, parsedDate)                       ' Noncompliant
        Date.TryParseExact("01/02/2000".AsSpan(), {"dd/MM/yyyy", "dd MM yyyy"}, CultureInfo.InvariantCulture, DateTimeStyles.None, parsedDate)  ' Compliant
    End Sub
End Class

""").Verify();

#endif

}
