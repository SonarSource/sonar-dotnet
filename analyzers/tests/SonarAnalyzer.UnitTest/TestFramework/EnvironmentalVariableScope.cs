/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Helpers
{
    /// <summary>
    /// Defines a scope inside which new environment variables can be set.
    /// The variables will be cleared when the scope is disposed.
    /// Copy-pasted from S4NET codebase.
    /// </summary>
    public sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly bool setOnlyInAzureDevOpsContext;
        private IDictionary<string, string> originalValues = new Dictionary<string, string>();

        public EnvironmentVariableScope(bool setVariablesOnlyInAzureDevOpsContext = false)
        {
            setOnlyInAzureDevOpsContext = setVariablesOnlyInAzureDevOpsContext;
        }

        public void SetVariable(string name, string value)
        {
            if (setOnlyInAzureDevOpsContext && !TestContextHelper.IsAzureDevOpsContext)
            {
                return;
            }
            // Store the original value, or null if there isn't one
            if (!originalValues.ContainsKey(name))
            {
                originalValues.Add(name, Environment.GetEnvironmentVariable(name));
            }
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        }

        #region IDispose implementation

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            disposed = true;

            if (disposing && originalValues != null)
            {
                foreach (var kvp in originalValues)
                {
                    Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                }

                originalValues = null;
            }
        }

        #endregion IDispose implementation
    }
}
