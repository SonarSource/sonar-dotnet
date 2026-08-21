/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.ShimLayer.Generator.Strategies;

public abstract class MemberStrategy : Strategy
{
    public IReadOnlyList<MemberDescriptor> Members { get; }

    protected MemberStrategy(Type latest, MemberDescriptor[] members) : base(latest) =>
        Members = members;

    protected MemberSnippets PassthroughMembers(StrategyModel model) =>
        new(
            CreateSnippets(x => x.IsPassthrough && ValidPropertyReturnType(model, x) is { } returnType ? new PropertyPassthroughSnippet(this, x, returnType) : null),
            CreateSnippets(x => x.IsPassthrough && ValidMethodReturnType(model, x) is { } returnType ? new MethodPassthroughSnippet(this, x, returnType, model) : null));

    protected MemberSnippets WrapMembers(StrategyModel model) =>
        new(
            CreateSnippets(x => !x.IsPassthrough && ValidPropertyReturnType(model, x) is { } returnType ? new PropertyWrapSnippet(this, x, returnType) : null),
            CreateSnippets(x => !x.IsPassthrough && ValidMethodReturnType(model, x) is { } returnType ? new MethodWrapSnippet(this, x, returnType, model) : null));

    private Snippet[] CreateSnippets(Func<MemberDescriptor, Snippet> selector) =>
        Members.Select(selector).Where(x => x is not null).ToArray();

    private static Strategy ValidPropertyReturnType(StrategyModel model, MemberDescriptor member) =>
        member.Member is PropertyInfo pi && model[pi.PropertyType] is { IsSupported: true } returnType ? returnType : null;

    private static Strategy ValidMethodReturnType(StrategyModel model, MemberDescriptor member) =>
        member.Member is MethodInfo { ContainsGenericParameters: false } mi
        && model[mi.ReturnType] is { IsSupported: true } returnType
        && mi.GetParameters().All(x => model[x.ParameterType].IsSupported)
            ? returnType
            : null;

    protected record struct MemberSnippets(Snippet[] Properties, Snippet[] Methods);
}
