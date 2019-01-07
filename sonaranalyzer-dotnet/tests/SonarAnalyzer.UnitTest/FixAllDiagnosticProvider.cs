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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;


namespace SonarAnalyzer.UnitTest
{
    /// /// <summary>
    /// FixAll context with some additional information specifically for <see cref="FixAllCodeAction"/>.
    /// </summary>
    internal class FixAllDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        private readonly ISet<string> _diagnosticIds;

        /// <summary>
        /// Delegate to fetch diagnostics for any given document within the given fix all scope.
        /// This delegate is invoked by <see cref="GetDocumentDiagnosticsAsync(Document, CancellationToken)"/> with the given <see cref="_diagnosticIds"/> as arguments.
        /// </summary>
        private readonly Func<Document, ISet<string>, CancellationToken, Task<IEnumerable<Diagnostic>>> _getDocumentDiagnosticsAsync;

        /// <summary>
        /// Delegate to fetch diagnostics for any given project within the given fix all scope.
        /// This delegate is invoked by <see cref="GetProjectDiagnosticsAsync(Project, CancellationToken)"/> and <see cref="GetAllDiagnosticsAsync(Project, CancellationToken)"/>
        /// with the given <see cref="_diagnosticIds"/> as arguments.
        /// The boolean argument to the delegate indicates whether or not to return location-based diagnostics, i.e.
        /// (a) False => Return only diagnostics with <see cref="Location.None"/>.
        /// (b) True => Return all project diagnostics, regardless of whether or not they have a location.
        /// </summary>
        private readonly Func<Project, bool, ISet<string>, CancellationToken, Task<IEnumerable<Diagnostic>>> _getProjectDiagnosticsAsync;

        public FixAllDiagnosticProvider(
            ISet<string> diagnosticIds,
            Func<Document, ISet<string>, CancellationToken, Task<IEnumerable<Diagnostic>>> getDocumentDiagnosticsAsync,
            Func<Project, bool, ISet<string>, CancellationToken, Task<IEnumerable<Diagnostic>>> getProjectDiagnosticsAsync)
        {
            this._diagnosticIds = diagnosticIds;
            this._getDocumentDiagnosticsAsync = getDocumentDiagnosticsAsync;
            this._getProjectDiagnosticsAsync = getProjectDiagnosticsAsync;
        }

        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            return this._getDocumentDiagnosticsAsync(document, this._diagnosticIds, cancellationToken);
        }

        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return this._getProjectDiagnosticsAsync(project, true, this._diagnosticIds, cancellationToken);
        }

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return this._getProjectDiagnosticsAsync(project, false, this._diagnosticIds, cancellationToken);
        }
    }
}
