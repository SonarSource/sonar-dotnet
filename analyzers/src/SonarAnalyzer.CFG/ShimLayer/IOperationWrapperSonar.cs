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

using System.Reflection;
using SonarAnalyzer.CFG;

namespace StyleCop.Analyzers.Lightup
{
    // This is a temporary substitute for IOperationWrapper in case StyleCop will accept PR https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3381
    public readonly struct IOperationWrapperSonar
    {
        private static readonly Func<IOperation, IOperation> ParentPropertyAccessor;
        private static readonly Func<IOperation, IEnumerable<IOperation>> ChildrenPropertyAccessor;
        private static readonly Func<IOperation, string> LanguagePropertyAccessor;
        private static readonly Func<IOperation, bool> IsImplicitPropertyAccessor;
        private static readonly Func<IOperation, SemanticModel> SemanticModelPropertyAccessor;

        static IOperationWrapperSonar()
        {
            var type = typeof(IOperation);
            ParentPropertyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, IOperation>(type, nameof(Parent));
            ChildrenPropertyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, IEnumerable<IOperation>>(type, nameof(Children));
            LanguagePropertyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, string>(type, nameof(Language));
            IsImplicitPropertyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, bool>(type, nameof(IsImplicit));
            SemanticModelPropertyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, SemanticModel>(type, nameof(SemanticModel));
        }

        public IOperation Instance { get; }
        public IOperation Parent => ParentPropertyAccessor(Instance);
        public IEnumerable<IOperation> Children => ChildrenPropertyAccessor(Instance);
        public string Language => LanguagePropertyAccessor(Instance);
        public bool IsImplicit => IsImplicitPropertyAccessor(Instance);
        public SemanticModel SemanticModel => SemanticModelPropertyAccessor(Instance);

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
