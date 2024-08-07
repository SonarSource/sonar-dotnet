/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace StyleCop.Analyzers.Lightup;

// This is a temporary substitute for IOperationWrapper in case StyleCop will accept PR https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3381
public readonly struct IOperationWrapperSonar
{
    private static readonly Func<IOperation, IOperation> ParentAccessor;
    private static readonly Func<IOperation, IEnumerable<IOperation>> ChildrenAccessor;
    private static readonly Func<IOperation, string> LanguageAccessor;
    private static readonly Func<IOperation, bool> IsImplicitAccessor;
    private static readonly Func<IOperation, SemanticModel> SemanticModelAccessor;

    public IOperation Instance { get; }
    public IOperation Parent => ParentAccessor(Instance);
    public IEnumerable<IOperation> Children => ChildrenAccessor(Instance);
    public string Language => LanguageAccessor(Instance);
    public bool IsImplicit => IsImplicitAccessor(Instance);
    public SemanticModel SemanticModel => SemanticModelAccessor(Instance);

    public IOperationWrapperSonar(IOperation instance) =>
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));

    static IOperationWrapperSonar()
    {
        var type = typeof(IOperation);
        ParentAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, IOperation>(type, nameof(Parent));
        ChildrenAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, IEnumerable<IOperation>>(type, nameof(Children));
        LanguageAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, string>(type, nameof(Language));
        IsImplicitAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, bool>(type, nameof(IsImplicit));
        SemanticModelAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IOperation, SemanticModel>(type, nameof(SemanticModel));
    }

    public override int GetHashCode() =>
        Instance.GetHashCode();

    public override bool Equals(object obj) =>
        obj is IOperationWrapperSonar wrapper && wrapper.Instance.Equals(Instance);

    public override string ToString() =>
        Instance.ToString();
}
