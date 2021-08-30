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
using System.Reflection;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.Roslyn
{
    // This is a temporary substitute for IOperationWrapper in case StyleCop will accept PR https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3381
    public class IOperationWrapperSonar
    {
        private static readonly PropertyInfo ParentProperty;
        private static readonly PropertyInfo ChildrenProperty;
        private static readonly PropertyInfo LanguageProperty;
        private static readonly PropertyInfo IsImplicitProperty;
        private static readonly PropertyInfo SemanticModelProperty;

        private readonly Lazy<IOperation> parent;
        private readonly Lazy<IEnumerable<IOperation>> children;
        private readonly Lazy<string> language;
        private readonly Lazy<bool> isImplicit;
        private readonly Lazy<SemanticModel> semanticModel;

        public IOperation Instance { get; }
        public IOperation Parent => parent.Value;
        public IEnumerable<IOperation> Children => children.Value;
        public string Language => language.Value;
        public bool IsImplicit => isImplicit.Value;
        public SemanticModel SemanticModel => semanticModel.Value;

        static IOperationWrapperSonar()
        {
            var type = typeof(IOperation);
            ParentProperty = type.GetProperty(nameof(Parent));
            ChildrenProperty = type.GetProperty(nameof(Children));
            LanguageProperty = type.GetProperty(nameof(Language));
            IsImplicitProperty = type.GetProperty(nameof(IsImplicit));
            SemanticModelProperty = type.GetProperty(nameof(SemanticModel));
        }

        public IOperationWrapperSonar(IOperation instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            parent = ParentProperty.ReadValue<IOperation>(instance);
            children = ChildrenProperty.ReadValue<IEnumerable<IOperation>>(instance);
            language = LanguageProperty.ReadValue<string>(instance);
            isImplicit = IsImplicitProperty.ReadValue<bool>(instance);
            semanticModel = SemanticModelProperty.ReadValue<SemanticModel>(instance);
        }
    }
}
