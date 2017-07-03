/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System;
using System.Collections.Generic;
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
    public sealed class OperatorsShouldBeOverloadedConsistently : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4050";
        private const string MessageFormat = "Provide an implementation for: {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static class MethodName
        {
            public const string OperatorPlus = "operator+";
            public const string OperatorMinus = "operator-";
            public const string OperatorEquals = "operator==";
            public const string OperatorNotEquals = "operator!=";

            public const string ObjectEquals = "Object.Equals";
            public const string ObjectGetHashCode = "Object.GetHashCode";
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classDeclaration = (ClassDeclarationSyntax)c.Node;
                var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                if (classSymbol == null || classDeclaration.Identifier.IsMissing)
                {
                    return;
                }

                var classMethods = classSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(m => !m.IsConstructor())
                    .ToList();

                var implementedMethods = new HashSet<string>();
                implementedMethods.Add(GetNameOrDefault(MethodName.OperatorPlus, classMethods, KnownMethods.IsOperatorBinaryPlus));
                implementedMethods.Add(GetNameOrDefault(MethodName.OperatorMinus, classMethods, KnownMethods.IsOperatorBinaryMinus));
                implementedMethods.Add(GetNameOrDefault(MethodName.OperatorEquals, classMethods, KnownMethods.IsOperatorEquals));
                implementedMethods.Add(GetNameOrDefault(MethodName.OperatorNotEquals, classMethods, KnownMethods.IsOperatorNotEquals));
                implementedMethods.Add(GetNameOrDefault(MethodName.ObjectEquals, classMethods, KnownMethods.IsObjectEquals));
                implementedMethods.Add(GetNameOrDefault(MethodName.ObjectGetHashCode, classMethods, KnownMethods.IsObjectGetHashCode));
                implementedMethods.Remove(null);

                var requiredMethods = new HashSet<string>();

                if (implementedMethods.Contains(MethodName.OperatorPlus))
                {
                    requiredMethods.Add(MethodName.OperatorMinus);
                    requiredMethods.Add(MethodName.OperatorEquals);
                    requiredMethods.Add(MethodName.OperatorNotEquals);
                    requiredMethods.Add(MethodName.ObjectEquals);
                    requiredMethods.Add(MethodName.ObjectGetHashCode);
                }

                if (implementedMethods.Contains(MethodName.OperatorMinus))
                {
                    requiredMethods.Add(MethodName.OperatorPlus);
                    requiredMethods.Add(MethodName.OperatorEquals);
                    requiredMethods.Add(MethodName.OperatorNotEquals);
                    requiredMethods.Add(MethodName.ObjectEquals);
                    requiredMethods.Add(MethodName.ObjectGetHashCode);
                }

                if (implementedMethods.Contains(MethodName.OperatorEquals))
                {
                    requiredMethods.Add(MethodName.OperatorNotEquals);
                    requiredMethods.Add(MethodName.ObjectEquals);
                    requiredMethods.Add(MethodName.ObjectGetHashCode);
                }

                if (implementedMethods.Contains(MethodName.OperatorNotEquals))
                {
                    requiredMethods.Add(MethodName.OperatorEquals);
                    requiredMethods.Add(MethodName.ObjectEquals);
                    requiredMethods.Add(MethodName.ObjectGetHashCode);
                }

                var missingMethods = requiredMethods.Except(implementedMethods);
                if (missingMethods.Any())
                {
                    string missingMethodsMessage = string.Join(", ", missingMethods.Select(s => $"'{s}'"));
                    c.ReportDiagnostic(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation(), missingMethodsMessage));
                }
            },
            SyntaxKind.ClassDeclaration);
        }

        private string GetNameOrDefault(string item, IList<IMethodSymbol> methods, Func<IMethodSymbol, bool> predicate)
        {
            return methods.FirstOrDefault(predicate) != null ? item : null;
        }
    }
}
