/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using System.Reflection;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG;

namespace StyleCop.Analyzers.Lightup
{
    // This is a temporary substitute for IOperationWrapper in case StyleCop will accept PR https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3381
    public class IOperationWrapperSonar
    {
        private static readonly PropertyInfo ParentProperty;
        private static readonly PropertyInfo ChildrenProperty;
        private static readonly PropertyInfo LanguageProperty;
        private static readonly PropertyInfo IsImplicitProperty;
        private static readonly PropertyInfo SemanticModelProperty;

        private IOperation parent;
        private IEnumerable<IOperation> children;
        private string language;
        private bool? isImplicit;
        private SemanticModel semanticModel;

        public IOperation Instance { get; }
        public IOperation Parent => ParentProperty.ReadCached(Instance, ref parent);
        public IEnumerable<IOperation> Children => ChildrenProperty.ReadCached(Instance, ref children);
        public string Language => LanguageProperty.ReadCached(Instance, ref language);
        public bool IsImplicit => IsImplicitProperty.ReadCached(Instance, ref isImplicit);
        public SemanticModel SemanticModel => SemanticModelProperty.ReadCached(Instance, ref semanticModel);

        static IOperationWrapperSonar()
        {
            var type = typeof(IOperation);
            ParentProperty = type.GetProperty(nameof(Parent));
            ChildrenProperty = type.GetProperty(nameof(Children));
            LanguageProperty = type.GetProperty(nameof(Language));
            IsImplicitProperty = type.GetProperty(nameof(IsImplicit));
            SemanticModelProperty = type.GetProperty(nameof(SemanticModel));
        }

        public IOperationWrapperSonar(IOperation instance) =>
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));

        public override int GetHashCode() =>
            Instance.GetHashCode();

        public override bool Equals(object obj) =>
            obj is IOperationWrapperSonar wrapper && wrapper.Instance.Equals(Instance);

        public override string ToString() =>
            Instance.ToString();
    }
}
