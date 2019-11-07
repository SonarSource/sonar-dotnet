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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CertificateValidationCheck : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4830";
        private const string MessageFormat = "Enable server certificate validation on this SSL/TLS connection";

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {



            //FIXME: Hunt for
            //property Func<Cokoliv = objekt nebo neco specifickeho, System::Security::Cryptography::X509Certificates::X509Certificate2 ^, System::Security::Cryptography::X509Certificates::X509Chain ^, System::Net::Security::SslPolicyErrors, bool>
            //and
            //RemoteCertificateValidationCallback


            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {

                    
                    var Xxx = ((AssignmentExpressionSyntax)c.Node).Left
                                .DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
                    //.ChildNodes().OfType<IdentifierNameSyntax>()
                    //.Last;
                    System.Diagnostics.Debugger.Break();

                    
                    //((AddAssignmentExpression)c.Node)

                    //var node = c.Node;
                    //if (true)
                    //{
                    //    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
                    //}




                }
                , SyntaxKind.AddAssignmentExpression
            );
        }
    }
}

