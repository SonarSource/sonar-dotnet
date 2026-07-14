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

using static System.Linq.Expressions.Expression;

namespace StyleCop.Analyzers.Lightup;

public static class SemanticModelExtensions
{
    private static readonly Func<SemanticModel, int, NullableContext> GetNullableContextAccessor = CreateGetNullableContextAccessor();

    public static NullableContext GetNullableContext(this SemanticModel model, int position) =>
        GetNullableContextAccessor(model, position);

    private static Func<SemanticModel, int, NullableContext> CreateGetNullableContextAccessor()
    {
        var method = typeof(SemanticModel).GetMethod(nameof(GetNullableContext), [typeof(int)]);
        if (method is null)
        {
            // SemanticModel.GetNullableContext(int) shipped in Roslyn 3.3.0 (VS 16.3, the final C#8/NRT release).
            // "Disabled" is the right default for any host older than that: pre-C#8 Roslyn (< 3.0) has no nullable
            // context at all, so warnings genuinely are off. Only the short-lived 3.0-3.2 previews (VS 16.0-16.2)
            // already parsed NRT syntax like "!" without exposing this API, so "Disabled" is technically wrong there
            // - those previews are long obsolete, so the wrong result is not reachable in practice today.
            return static (_, _) => NullableContext.Disabled;
        }

        var modelParameter = Parameter(typeof(SemanticModel), "model");
        var positionParameter = Parameter(typeof(int), "position");
        var body = Convert(Call(modelParameter, method, positionParameter), typeof(NullableContext));
        var expression = Lambda<Func<SemanticModel, int, NullableContext>>(body, modelParameter, positionParameter);
        return expression.Compile();
    }
}
